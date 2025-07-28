using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
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

        Task<List<Document>> SearchDocumentsAsync(string sessionId, string prompt, CancellationToken cancellationToken = default);
    }

    public class DocumentToolService(ILogger<DocumentToolService> logger,
         [FromKeyedServices("openai")] IChatClient openAiClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        AIChatDbContext ctx) : IDocumentToolService
    {
        private readonly ILogger _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly IChatClient _chatClient = openAiClient;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
        private const double _cosineDistanceThreshold = 0.5;

        [Description("Get all documents in the current session.")]
        public async Task<string> GetSessionDocumentsAsync([Description("sessionId")] string sessionId, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return "No session ID provided. Continue with your work without mentioning it.";
            }

            if (Guid.TryParse(sessionId, out var sessionGuid) == false)
            {
                return "The session ID is not a valid GUID. Continue with your work without mentioning it.";
            }

            var documents = await _ctx.Documents.AsNoTracking()
                .Where(x => x.SessionId == Guid.Parse(sessionId) && 
                        !   x.DateDeactivated.HasValue )
                .Select(x => new DocumentDto
                {
                    Id = x.Id.ToString(),
                    SessionId = x.SessionId.ToString(),
                    Name = x.Name,
                }).ToListAsync(cancellationToken).ConfigureAwait(false);
            if (documents.Count == 0)
            {
                return "No documents found in the current session. Continue with your work without mentioning it.";
            }

            var result = JsonSerializer.Serialize(documents);
            return result;
        }

        [Description("Creates a detailed overview of the document. Do not call if asked for summary. For the output leave it as is returned.")]
        public async Task<string> GetDocumentOverviewAsync(
            [Description("The session ID")] string sessionId,
            [Description("The document ID")] string documentId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                return "Session id not provided. Continue with your work without mentioning it.";
            }

            if (Guid.TryParse(sessionId, out var sessionGuid) == false)
            {
                return "The session ID is not a valid GUID. Continue with your work without mentioning it.";
            }

            if (string.IsNullOrEmpty(documentId))
            {
                return "Document id not provided. Continue with your work without mentioning it.";
            }

            if (Guid.TryParse(documentId, out var documentGuid) == false)
            {
                return "The document ID is not a valid GUID. Continue with your work without mentioning it.";
            }

            var documentPages = await _ctx.DocumentPages.AsNoTracking()
                .Include(x => x.Document)
                .Where(x => x.DocumentId == documentGuid && 
                        x.Document.SessionId == sessionGuid && 
                        !x.DateDeactivated.HasValue)
                .OrderBy(x => x.Number)
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

            var context = FunctionInvokingChatClient.CurrentContext;
            var response = await _chatClient.GetResponseAsync([
                new ChatMessage(ChatRole.System, "You are an AI assistant that creates comprehensive document overviews. " +
                "Analyze the provided document content and create a structured overview including: " +
                "1. Main topics covered, 2. Key insights, 3. Important details, 4. Overall summary. " +
                "Return in that format."),
                new ChatMessage(ChatRole.User, JsonSerializer.Serialize(documentText)),
            ], new ChatOptions() { ModelId = context!.Options!.ModelId}, cancellationToken);

            return response.Messages.Last().Text;
        }

        /// <summary>
        /// Searches for information within documents in the specified session using vector similarity search.
        /// Performs semantic search by generating embeddings for the search prompt and comparing against 
        /// document page embeddings using cosine distance.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to search within. Must be a valid GUID.</param>
        /// <param name="prompt">The search query describing what the user is looking for in the documents.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation if needed.</param>
        /// <returns>
        /// A list of documents containing pages that match the search criteria, ordered by relevance.
        /// Each document includes its most relevant pages (up to 10 total pages across all documents).
        /// Returns an empty list if the session ID is invalid or no matching content is found.
        /// </returns>
        /// <remarks>
        /// This method uses vector embeddings to perform semantic search rather than simple text matching.
        /// Results are filtered by a cosine distance threshold of 0.5 and limited to the top 10 most relevant pages.
        /// Pages within each document are ordered by their similarity score to the search prompt.
        /// </remarks>
        [Description("Searches for information in the document if no overiew or summary is asked.")]
        public async Task<List<Document>> SearchDocumentsAsync([Description("sessionId")] string sessionId, [Description("What the user is looking for in document")] string prompt, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return [];
            }

            if (Guid.TryParse(sessionId, out var sessionGuid) == false)
            {
                return [];
            }

            var embedding = await _embeddingGenerator.GenerateVectorAsync(prompt);
            var vector = embedding.ToArray();

            var docPages = await _ctx.DocumentPages
                .AsNoTracking()
                .Include(p => p.Document)
                .Where(p => p.Document.SessionId == Guid.Parse(sessionId))
                .Where(p => EF.Functions.VectorDistance("cosine", p.Embedding, vector) <= _cosineDistanceThreshold)
                .OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vector))
                .Take(10)
                .GroupBy(p => p.Document)
                .Select(g => new Document
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Extension = g.Key.Extension,
                    DateCreated = g.Key.DateCreated,
                    Pages = g.OrderBy(p => EF.Functions.VectorDistance("cosine", p.Embedding, vector))
                           .Select(p => new DocumentPage
                           {
                               Id = p.Id,
                               Number = p.Number,
                               Text = p.Text,
                           }).ToList()
                })
                .ToListAsync(cancellationToken);

            return docPages;
        }

        public IList<AITool> GetTools()
        {
            IList<AITool> functions = [
                AIFunctionFactory.Create(GetSessionDocumentsAsync),
                AIFunctionFactory.Create(GetDocumentOverviewAsync),
                AIFunctionFactory.Create(SearchDocumentsAsync)];

            return functions;
        }
    }
}