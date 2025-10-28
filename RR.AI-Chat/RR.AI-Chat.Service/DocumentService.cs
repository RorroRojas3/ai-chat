using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using RR.AI_Chat.Service.Extensions;

namespace RR.AI_Chat.Service
{
    public interface IDocumentService 
    {
        Task<DocumentDto> CreateDocumentAsync(PerformContext? context, FileDto fileDataDto, Guid userId, Guid sessionId, CancellationToken cancellationToken);

        Task<FileDto?> GenerateConversationHistoryAsync(Guid sessionId, DocumentFormats documentFormat, CancellationToken cancellationToken);

        Task<List<DocumentExtractorDto>> ExtractTextAsync(byte[] bytes, CancellationToken cancellationToken);

        Task<PageEmbeddingDto> GeneratePageEmbedding(DocumentExtractorDto documentExtractor, CancellationToken cancellationToken);
    }

    public class DocumentService(ILogger<DocumentService> logger, 
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IBlobStorageService blobStorageService,
        IConfiguration configuration,
        IDocumentIntelligenceService documentIntelligenceService,
        ITokenService tokenService,
        IHtmlService htmlService,
        IPdfService pdfService,
        IWordService wordService,
        IMarkdownService markdownService,
        AIChatDbContext ctx) : IDocumentService
    {
        private readonly ILogger _logger = logger;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
        private readonly IBlobStorageService _blobStorageService = blobStorageService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IDocumentIntelligenceService _documentIntelligenceService = documentIntelligenceService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IHtmlService _htmlService = htmlService;
        private readonly IPdfService _pdfService = pdfService;
        private readonly IWordService _wordService = wordService;
        private readonly IMarkdownService _markdownService = markdownService;
        private readonly AIChatDbContext _ctx = ctx;
        private const double _cosineDistanceThreshold = 0.3;

        /// <summary>
        /// Creates a new document asynchronously by extracting text from a PDF file, generating embeddings for each page, and storing the document in the database.
        /// </summary>
        /// <param name="formFile">The uploaded PDF file to process. Must not be null.</param>
        /// <param name="sessionId">The unique identifier of the session to associate the document with.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="DocumentDto"/> with the created document's ID and name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formFile"/> is null.</exception>
        /// <remarks>
        /// This method performs the following operations:
        /// 1. Reads the uploaded file as a byte array
        /// 2. Extracts text from each page of the PDF
        /// 3. Generates vector embeddings for each page's text
        /// 4. Creates document page entities with embeddings
        /// 5. Saves the document and all pages to the database
        /// </remarks>
        public async Task<DocumentDto> CreateDocumentAsync(PerformContext? context, FileDto fileDataDto, Guid userId, Guid sessionId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(fileDataDto, nameof(fileDataDto));

            var jobId = context.BackgroundJob.Id;
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Queued.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 0);
            _logger.LogInformation("Starting document creation. Job ID: {JobId}, Session ID: {SessionId}, File Name: {FileName}", jobId, sessionId, fileDataDto.FileName);

            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Uploading.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 25);

            string container = _configuration.GetValue<string>("AzureStorage:DocumentsContainer")!;  
            string blob = $"{userId}/{sessionId}/{fileDataDto.FileName}";
            Dictionary<string, string> metadata = new()
            {
                { "userId", userId.ToString() },
                { "sessionId", sessionId.ToString() },
                { "fileName", fileDataDto.FileName },
                { "contentType", fileDataDto.ContentType },
                { "length", fileDataDto.Length.ToString() }
            };

            await _blobStorageService.UploadAsync(container, blob, fileDataDto.Content, metadata, cancellationToken);

            var documentExtractors = await ExtractTextAsync(fileDataDto.Content, cancellationToken);
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Extracting.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 50);

            List<DocumentPage> documentPages = [];
            var date = DateTime.UtcNow;

            var tasks = new List<Task<PageEmbeddingDto>>();

            foreach (var documentExtractor in documentExtractors)
            {
                var task = GeneratePageEmbedding(documentExtractor, cancellationToken);
                tasks.Add(task);

                // Process in batches of 10
                if (tasks.Count == 10)
                {
                    var completedTasks = await Task.WhenAll(tasks);
                    foreach (var result in completedTasks)
                    {
                        documentPages.Add(new DocumentPage
                        {
                            Number = result.Number,
                            Embedding = result.Embedding.ToArray(),
                            Text = result.Text,
                            DateCreated = date
                        });
                    }
                    tasks.Clear();
                }
            }

            // Process remaining tasks (less than 10)
            if (tasks.Count > 0)
            {
                var completedTasks = await Task.WhenAll(tasks);
                foreach (var result in completedTasks)
                {
                    documentPages.Add(new DocumentPage
                    {
                        Number = result.Number,
                        Embedding = result.Embedding.ToArray(),
                        Text = result.Text,
                        DateCreated = date
                    });
                }
            }
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Embedding.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 75);

            var document = new Document
            {
                UserId = userId,
                SessionId = sessionId,
                Name = fileDataDto.FileName,
                Extension = GetFileExtension(fileDataDto.FileName),
                MimeType = fileDataDto.ContentType,
                Size = fileDataDto.Length,
                Path = blob,
                Pages = documentPages,
                DateCreated = date
            };

            await _ctx.AddAsync(document, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Processed.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 100);

            return new()
            {
                Id = document.Id.ToString(),
                Name = document.Name
            };
        }

        /// <summary>
        /// Reads all bytes from the provided stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream to read bytes from.</param>
        /// <returns>A task that represents the asynchronous read operation. The task result contains the byte array of the stream content.</returns>
        public static async Task<byte[]> ReadAllBytesAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Gets the file extension from the provided file name.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The file extension.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the file name is null or empty.</exception>
        public static string GetFileExtension(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName, nameof(fileName));
            var extension = Path.GetExtension(fileName);
            return extension;
        }


        /// <summary>
        /// Extracts text from a PDF file asynchronously.
        /// </summary>
        /// <param name="bytes">The byte array representing the PDF file.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the extracted text from the PDF file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the byte array is null.</exception>
        public async Task<List<DocumentExtractorDto>> ExtractTextAsync(byte[] bytes, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(bytes, nameof(bytes));

            var analyzeResult = await _documentIntelligenceService.ReadAsync(bytes, cancellationToken);

            var dto = analyzeResult.Pages.Select(page => new DocumentExtractorDto
            {
                PageNumber = page.PageNumber,
                PageText = string.Join("\n", page.Lines.Select(line => line.Content))
            }).ToList();

            return dto;
        }

        public async Task<FileDto?> GenerateConversationHistoryAsync(Guid sessionId, DocumentFormats documentFormat, CancellationToken cancellationToken)
        {
            var oid = _tokenService.GetOid()!;

            var session = await _ctx.Sessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == sessionId &&
                                    x.UserId == oid.Value &&
                                    x.Conversations != null &&
                                    !x.DateDeactivated.HasValue,
                                    cancellationToken);

            if (session?.Conversations == null || session.Conversations.Count == 0)
            {
                return null;
            }

            var conversations = session.Conversations
                                      .Where(x => x.Role != ChatRole.System)
                                      .ToList();

            if (conversations.Count == 0)
            {
                return null;
            }

            var html = _htmlService.GenerateConversationHistoryAsync(conversations);
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            byte[]? bytes = documentFormat switch
            {
                DocumentFormats.Pdf => _pdfService.GeneratePdfFromHtml(html),
                DocumentFormats.Word => _wordService.GenerateWordFromHtml(html),
                DocumentFormats.Markdown => _markdownService.GenerateMarkdownFromHtml(html),
                _ => null
            };
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            var fileName = documentFormat switch
            {
                DocumentFormats.Pdf => $"conversation-history-{sessionId}.pdf",
                DocumentFormats.Word => $"conversation-history-{sessionId}.docx",
                DocumentFormats.Markdown => $"conversation-history-{sessionId}.md",
                _ => null
            };
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            return new FileDto
            {
                FileName = fileName,
                Content = bytes,
                ContentType = documentFormat.GetDescription(),
                Length = bytes.Length
            };
        }

        public async Task<PageEmbeddingDto> GeneratePageEmbedding(DocumentExtractorDto documentExtractor, CancellationToken cancellationToken)
        {
            var embedding = await _embeddingGenerator.GenerateVectorAsync(string.IsNullOrWhiteSpace(documentExtractor.PageText) ? "EMPTY PAGE" : documentExtractor.PageText, null, cancellationToken);
            return new PageEmbeddingDto
            {
                Number = documentExtractor.PageNumber,
                Embedding = embedding,
                Text = documentExtractor.PageText
            };
        }
    }
}
