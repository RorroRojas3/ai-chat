using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using System.ComponentModel;
using System.Text.Json;

namespace RR.AI_Chat.Service
{
    public interface IDocumentToolService
    {
        Task<string> GetSessionDocumentsAsync(string sessionId, CancellationToken cancellationToken = default);

        Task<string> GetDocumentOverviewAsync(string sessionId, string documentId, CancellationToken cancellationToken = default);
    
        IList<AITool> GetTools();   
    }

    public class DocumentToolService(ILogger<DocumentToolService> logger, 
        IHttpContextAccessor httpContextAccessor,
        IChatClient chatClient,
        AIChatDbContext ctx) : IDocumentToolService
    {
        private readonly ILogger _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IChatClient _chatClient = chatClient;

        [Description("Get all documents in the current session.")]
        public async Task<string> GetSessionDocumentsAsync([Description("sessionId")] string sessionId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return "No session ID provided.";
            }

            if (Guid.TryParse(sessionId, out var sessionGuid) == false)
            {
                return "The session ID is not a valid GUID.";
            }

            var documents = await _ctx.Documents.AsNoTracking()
                .Where(x => x.SessionId == Guid.Parse(sessionId))
                .Select(x => new DocumentDto
                {
                    Id = x.Id.ToString(),
                    SessionId = x.SessionId.ToString(),
                    Name = x.Name,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (documents.Count == 0)
            {
                return "No documents found in the current session.";
            }

            var result = JsonSerializer.Serialize(documents);
            return result;
        }

        [Description("Create a detailed overview of a specific document.")]
        public async Task<string> GetDocumentOverviewAsync(
            [Description("The session ID")] string sessionId,
            [Description("The document ID")] string documentId,
            CancellationToken cancellationToken = default)
        {
            var documentPages = await _ctx.DocumentPages.AsNoTracking()
                .Include(x => x.Document)
                .Where(x => x.DocumentId == Guid.Parse(documentId) && x.Document.SessionId == Guid.Parse(sessionId))
                .OrderBy(x => x.Number)
                .Skip(0)
                .Take(15)
                .Select(x => new DocumentPage
                {
                    Id = x.Id,
                    Number = x.Number,
                    Text = x.Text,
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var documentText = string.Join("\n\n", documentPages.Select(p =>
           $"Page {p.Number}: {p.Text}"));

            var response = await _chatClient.GetResponseAsync([
                new ChatMessage(ChatRole.System, "You are an AI assistant that creates comprehensive document overviews. " +
                "Analyze the provided document content and create a structured summary including: " +
                "1. Main topics covered, 2. Key insights, 3. Important details, 4. Overall summary."),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(documentText)),
            ], null, cancellationToken);

            return response.Messages.Last().Text;
        }

        public IList<AITool> GetTools()
        {
            IList<AITool> functions = [
                AIFunctionFactory.Create(GetSessionDocumentsAsync),
                AIFunctionFactory.Create(GetDocumentOverviewAsync)];

            return functions;
        }
    }
}