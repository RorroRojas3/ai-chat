using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
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

        Task<string> CompareDocumentsAsync(string sessionId, string firstDocumentId, string secondDocumentId, CancellationToken cancellationToken = default);
    }

    public class DocumentToolService(ILogger<DocumentToolService> logger,
         [FromKeyedServices("azureaifoundry")] IChatClient openAiClient,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IBlobStorageService blobStorageService,
        IConfiguration configuration,
        ITokenService tokenService,
        AIChatDbContext ctx) : IDocumentToolService
    {
        private readonly ILogger _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly IChatClient _chatClient = openAiClient;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
        private readonly IBlobStorageService _blobStorageService = blobStorageService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ITokenService _tokenService = tokenService;    
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

        [Description("Returns the complete text content of the document for AI processing. Use when full document content is needed. Do not use for comparison.")]
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

            var documentText = string.Join("\n\n", documentPages.Select(p => p.Text));

            return documentText;
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

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return [];
            }

            var anyDocuments = await _ctx.Documents
                .AsNoTracking()
                .AnyAsync(d => d.SessionId == sessionGuid && !d.DateDeactivated.HasValue, cancellationToken);   
            if (!anyDocuments)
            {
                return [];
            }

            var embedding = await _embeddingGenerator.GenerateVectorAsync(prompt, null, cancellationToken);
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

        [Description("Compares two documents and provides detailed analysis of similarities and differences. " +
            "Use when user requests document comparison. " +
            "Return the output exactly as provided.")]
        public async Task<string> CompareDocumentsAsync(
            [Description("The session ID")] string sessionId,
            [Description("The ID of the first document to compare")] string firstDocumentId,
            [Description("The ID of the second document to compare")] string secondDocumentId,
            CancellationToken cancellationToken = default)
        {
            // Validate session ID
            if (string.IsNullOrWhiteSpace(sessionId) || !Guid.TryParse(sessionId, out var sessionGuid))
            {
                return "The session ID is not a valid.";
            }

            // Validate first document ID
            if (string.IsNullOrWhiteSpace(firstDocumentId) || !Guid.TryParse(firstDocumentId, out var firstDocGuid))
            {
                return "The first document ID is not a valid.";
            }

            // Validate second document ID
            if (string.IsNullOrEmpty(secondDocumentId) || !Guid.TryParse(secondDocumentId, out var secondDocGuid))
            {
                return "The second document ID is not a valid.";
            }


            // Retrieve first document content
            var firstDocumentText = await GetDocumentContentAsync(sessionGuid, firstDocGuid, cancellationToken);
            if (string.IsNullOrEmpty(firstDocumentText))
            {
                return "First document not found or has no content. Continue with your work without mentioning it.";
            }

            // Retrieve second document content
            var secondDocumentText = await GetDocumentContentAsync(sessionGuid, secondDocGuid, cancellationToken);
            if (string.IsNullOrEmpty(secondDocumentText))
            {
                return "Second document not found or has no content. Continue with your work without mentioning it.";
            }

            // Create detailed system prompt for comparison
            var systemPrompt = $@"You are an expert document analyst. Your task is to perform a comprehensive comparison between two documents and provide a detailed analysis.

                COMPARISON REQUIREMENTS:
                1. **Structural Analysis**: Compare document organization, sections, headings, formatting patterns
                2. **Content Analysis**: Identify shared themes, topics, concepts, and subject matter
                3. **Similarities**: Highlight common elements, shared information, parallel sections, similar language or terminology
                4. **Differences**: Point out contrasting viewpoints, unique content, different approaches, varying details
                5. **Key Insights**: Provide analytical insights about the relationship between documents
                6. **Quantitative Assessment**: Estimate percentage of content overlap where applicable
                7. **Qualitative Assessment**: Describe the nature and significance of differences

                ANALYSIS STRUCTURE:
                - Executive Summary
                - Detailed Similarities 
                - Detailed Differences
                - Structural Comparison
                - Content Themes Analysis
                - Recommendations or Conclusions

                Be thorough, objective, and provide specific examples from both documents to support your analysis.";

            var userPrompt = $@"Please compare the following two documents:

                === DOCUMENT 1 ===
                {firstDocumentText}

                === DOCUMENT 2 ===
                {secondDocumentText}

                Provide a comprehensive comparison analysis following the requirements specified in the system prompt.";

            try
            {
                // Use IChatClient to get comparison analysis
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, systemPrompt),
                    new(ChatRole.User, userPrompt)
                };

                var response = await _chatClient.GetResponseAsync(messages, null, cancellationToken).ConfigureAwait(false);
                return response.Messages.LastOrDefault()?.Text ?? "Failed to generate comparison analysis.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while comparing documents {firstDocId} and {secondDocId} in session {sessionId}",
                    firstDocumentId, secondDocumentId, sessionId);
                return "An error occurred while comparing the documents. Please try again.";
            }
        }

        private async Task<string?> GetDocumentContentAsync(Guid sessionId, Guid documentId, CancellationToken cancellationToken)
        {
            var documentPages = await _ctx.DocumentPages.AsNoTracking()
                .Include(x => x.Document)
                .Where(x => x.DocumentId == documentId &&
                        x.Document.SessionId == sessionId &&
                        !x.DateDeactivated.HasValue)
                .OrderBy(x => x.Number)
                .Select(x => x.Text)
                .ToListAsync(cancellationToken);

            return documentPages.Count > 0 ? string.Join("\n\n", documentPages) : null;
        }

        [Description("Uploads AI-generated files (documents, reports, spreadsheets) to Azure Blob Storage and returns a downloadable SAS URL. Use this after the Document Generation MCP server creates a file that needs to be stored and made available to the user.")]
        public async Task<Uri> ProcessGeneratedFileAsync(
            [Description("The unique identifier of the current chat session")] Guid sessionId, 
            [Description("The unique identifier of the user who owns the generated file")] Guid userId,
            [Description("The temporary SAS URI pointing to the AI-generated file in Azure Blob Storage that needs to be processed and stored permanently")] Uri sasUri)
        {
            ArgumentNullException.ThrowIfNull(sasUri);

            var blobClient = new BlobClient(sasUri);
            string blobName = blobClient.Name; 
            string filename = Path.GetFileName(blobName); 

            var response = await blobClient.DownloadContentAsync();

            var container = _configuration["AzureStorage:DocumentsContainer"]!;
            var path = $"{userId}/{sessionId}/{filename}";

            await _blobStorageService.UploadAsync(container, path, response.Value.Content.ToArray(), new Dictionary<string, string>
            {
                { "sessionId", sessionId.ToString() },
                { "userId", userId.ToString() }
            }, CancellationToken.None);

            var finalSasUri = _blobStorageService.GenerateSasUri(
                container,
                path,
                TimeSpan.FromHours(1),
                BlobSasPermissions.Read);

            return finalSasUri;
        }

        public IList<AITool> GetTools()
        {
            IList<AITool> functions = [
                AIFunctionFactory.Create(GetSessionDocumentsAsync),
                AIFunctionFactory.Create(GetDocumentOverviewAsync),
                AIFunctionFactory.Create(SearchDocumentsAsync),
                AIFunctionFactory.Create(CompareDocumentsAsync),
                AIFunctionFactory.Create(ProcessGeneratedFileAsync)];

            return functions;
        }
    }
}