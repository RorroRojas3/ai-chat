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
    public interface IDocumentFunctionService
    {
        Task<string> GetSessionDocumentsAsync(CancellationToken cancellationToken = default);

        Task<string> GetDocumentOverviewAsync(Guid sessionId, Guid documentId, CancellationToken cancellationToken = default);
    }

    public class DocumentFunctionService(ILogger<DocumentFunctionService> logger, 
        IHttpContextAccessor httpContextAccessor,
        IChatClient chatClient,
        AIChatDbContext ctx) : IDocumentFunctionService
    {
        private readonly ILogger _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IChatClient _chatClient = chatClient;

        [Description("Get all documents in the current session.")]
        public async Task<string> GetSessionDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var sessionId = _httpContextAccessor.HttpContext?.Request.Headers["sessionId"].FirstOrDefault();

            var documents = await _ctx.Documents.AsNoTracking()
                .Where(x => x.SessionId == Guid.Parse(sessionId!))
                .Select(x => new DocumentDto
                {
                    Id = x.Id,
                    SessionId = x.SessionId,
                    Name = x.Name,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var result = $"The documents in the current session are {JsonSerializer.Serialize(documents)}";
            return result;
        }

        [Description("Create a detailed overview of a specific document.")]
        public async Task<string> GetDocumentOverviewAsync(
            [Description("The session ID")] Guid sessionId,
            [Description("The document ID to create overview for")] Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var documentPages = await _ctx.Documents.AsNoTracking()
                .Where(x => x.Id == documentId && x.SessionId == sessionId)
                .Skip(0)
                .Take(15)
                .SelectMany(x => x.Pages.Select(x => new DocumentPage
                {
                    Id = x.Id,
                    Number = x.Number,
                    Text = x.Text,
                }).ToList())
                .ToListAsync()
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
    }
}