using Hangfire.Server;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Common.Enums;
using RR.AI_Chat.Common.Extensions;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using RR.AI_Chat.Service.Common.Interface;

namespace RR.AI_Chat.Service
{
    public interface IDocumentService 
    {
        Task<SessionDocumentDto> CreateSessionDocumentAsync(PerformContext? context, FileDto fileDataDto, Guid userId, Guid sessionId, CancellationToken cancellationToken);

        Task<FileDto?> GenerateConversationHistoryAsync(Guid sessionId, DocumentFormats documentFormat, CancellationToken cancellationToken);

        Task<List<DocumentExtractorDto>> ExtractTextAsync(FileDto fileDto, CancellationToken cancellationToken);

        Task<PageEmbeddingDto> GeneratePageEmbeddingAsync(DocumentExtractorDto documentExtractor, CancellationToken cancellationToken);

        Task<ProjectDocumentDto> CreateProjectDocumentAsync(PerformContext? context, FileDto fileDataDto, Guid projectId, CancellationToken cancellationToken);
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
        [FromKeyedServices("excel")] IFileService excelService,
        [FromKeyedServices("common")] IFileService commonFileService,
        [FromKeyedServices("word")] IFileService wordFileService,
        IAzureCosmosService cosmosService,
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
        private readonly IFileService _excelService = excelService;
        private readonly IFileService _commonFileService = commonFileService;
        private readonly IFileService _wordFileService = wordFileService;
        private readonly IAzureCosmosService _cosmosService = cosmosService;
        private readonly AIChatDbContext _ctx = ctx;

        /// <summary>
        /// Creates a new document asynchronously by extracting text from a PDF file, generating embeddings for each page, and storing the document in the database.
        /// </summary>
        /// <param name="formFile">The uploaded PDF file to process. Must not be null.</param>
        /// <param name="sessionId">The unique identifier of the session to associate the document with.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SessionDocumentDto"/> with the created document's ID and name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formFile"/> is null.</exception>
        /// <remarks>
        /// This method performs the following operations:
        /// 1. Reads the uploaded file as a byte array
        /// 2. Extracts text from each page of the PDF
        /// 3. Generates vector embeddings for each page's text
        /// 4. Creates document page entities with embeddings
        /// 5. Saves the document and all pages to the database
        /// </remarks>
        public async Task<SessionDocumentDto> CreateSessionDocumentAsync(PerformContext? context, FileDto fileDataDto, Guid userId, Guid sessionId, CancellationToken cancellationToken)
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

            var documentExtractors = await ExtractTextAsync(fileDataDto, cancellationToken);
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Extracting.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 50);

            List<ChatDocumentPage> documentPages = [];
            var date = DateTimeOffset.UtcNow;

            var tasks = new List<Task<PageEmbeddingDto>>();

            foreach (var documentExtractor in documentExtractors)
            {
                var task = GeneratePageEmbeddingAsync(documentExtractor, cancellationToken);
                tasks.Add(task);

                // Process in batches of 10
                if (tasks.Count == 10)
                {
                    var completedTasks = await Task.WhenAll(tasks);
                    foreach (var result in completedTasks)
                    {
                        documentPages.Add(new ChatDocumentPage
                        {
                            Number = result.Number,
                            Embedding = new SqlVector<float>(result.Embedding),
                            Text = result.Text,
                            DateCreated = date,
                            DateModified = date
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
                    documentPages.Add(new ChatDocumentPage
                    {
                        Number = result.Number,
                        Embedding = new SqlVector<float>(result.Embedding),
                        Text = result.Text,
                        DateCreated = date,
                        DateModified = date
                    });
                }
            }
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Embedding.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 75);

            var document = new ChatDocument
            {
                UserId = userId,
                SessionId = sessionId,
                Name = fileDataDto.FileName,
                Extension = GetFileExtension(fileDataDto.FileName),
                MimeType = fileDataDto.ContentType,
                Size = fileDataDto.Length,
                Path = blob,
                Pages = documentPages,
                DateCreated = date,
                DateModified = date
            };

            await _ctx.AddAsync(document, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Processed.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 100);

            return document.MapToSessionDocumentDto();
        }

        public async Task<FileDto?> GenerateConversationHistoryAsync(Guid sessionId, DocumentFormats documentFormat, CancellationToken cancellationToken)
        {
            var oid = _tokenService.GetOid();

            var chat = await _cosmosService.GetItemAsync<CosmosChat>(sessionId.ToString(), oid.ToString(), cancellationToken);
            if (chat == null)
            {
                throw new InvalidOperationException($"Chat with id {sessionId} not found.");
            }

            var html = _htmlService.GenerateConversationHistoryAsync(chat.Conversations);
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

        public async Task<PageEmbeddingDto> GeneratePageEmbeddingAsync(DocumentExtractorDto documentExtractor, CancellationToken cancellationToken)
        {
            var embedding = await _embeddingGenerator.GenerateVectorAsync(string.IsNullOrWhiteSpace(documentExtractor.PageText) ? "EMPTY PAGE" : documentExtractor.PageText, null, cancellationToken);
            return new PageEmbeddingDto
            {
                Number = documentExtractor.PageNumber,
                Embedding = embedding,
                Text = documentExtractor.PageText
            };
        }

        public async Task<ProjectDocumentDto> CreateProjectDocumentAsync(PerformContext? context, FileDto fileDataDto, Guid projectId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(fileDataDto, nameof(fileDataDto));

            var userId = _tokenService.GetOid();
            var isProjectExist = await _ctx.Projects
                                    .Where(x => x.Id == projectId &&
                                                x.UserId == userId &&
                                                !x.DateDeactivated.HasValue)
                                    .AnyAsync(cancellationToken);
            if (!isProjectExist)
            {
                _logger.LogError("Project does not exist.");
                throw new InvalidOperationException("Project does not exist.");
            }

            var jobId = context.BackgroundJob.Id;
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Queued.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 0);
            _logger.LogInformation("Starting document creation. Job ID: {JobId}, Project Id: {ProjectId}, File Name: {FileName}", jobId, projectId, fileDataDto.FileName);

            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Uploading.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 25);

            string container = _configuration.GetValue<string>("AzureStorage:DocumentsContainer")!;
            string blob = $"{userId}/{projectId}/{fileDataDto.FileName}";
            Dictionary<string, string> metadata = new()
            {
                { "userId", userId.ToString() },
                { "projectId", projectId.ToString() },
                { "fileName", fileDataDto.FileName },
                { "contentType", fileDataDto.ContentType },
                { "length", fileDataDto.Length.ToString() }
            };

            await _blobStorageService.UploadAsync(container, blob, fileDataDto.Content, metadata, cancellationToken);

            var documentExtractors = await ExtractTextAsync(fileDataDto, cancellationToken);
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Extracting.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 50);

            List<ProjectDocumentPage> documentPages = [];
            var date = DateTimeOffset.UtcNow;

            var tasks = new List<Task<PageEmbeddingDto>>();

            foreach (var documentExtractor in documentExtractors)
            {
                var task = GeneratePageEmbeddingAsync(documentExtractor, cancellationToken);
                tasks.Add(task);

                // Process in batches of 10
                if (tasks.Count == 10)
                {
                    var completedTasks = await Task.WhenAll(tasks);
                    foreach (var result in completedTasks)
                    {
                        documentPages.Add(new ProjectDocumentPage
                        {
                            Number = result.Number,
                            Embedding = new SqlVector<float>(result.Embedding),
                            Text = result.Text,
                            DateCreated = date,
                            DateModified = date,
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
                    documentPages.Add(new ProjectDocumentPage
                    {
                        Number = result.Number,
                        Embedding = new SqlVector<float>(result.Embedding),
                        Text = result.Text,
                        DateCreated = date,
                        DateModified = date,
                    });
                }
            }
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Embedding.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 75);

            var document = new ProjectDocument
            {
                UserId = userId,
                ProjectId = projectId,
                Name = fileDataDto.FileName,
                Extension = GetFileExtension(fileDataDto.FileName),
                MimeType = fileDataDto.ContentType,
                Size = fileDataDto.Length,
                Path = blob,
                Pages = documentPages,
                DateCreated = date,
                DateModified = date
            };

            await _ctx.AddAsync(document, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Processed.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 100);

            return document.MapToProjectDocumentDto();
        }

        #region Static methods
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
        public async Task<List<DocumentExtractorDto>> ExtractTextAsync(FileDto fileDto, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(fileDto, nameof(fileDto));

            List<DocumentExtractorDto> dto = [];
            if (fileDto.FileExtension == FileExtensions.Cs ||
                fileDto.FileExtension == FileExtensions.Csproj ||
                fileDto.FileExtension == FileExtensions.Css ||
                fileDto.FileExtension == FileExtensions.Html ||
                fileDto.FileExtension == FileExtensions.Js ||
                fileDto.FileExtension == FileExtensions.Json ||
                fileDto.FileExtension == FileExtensions.Jsx ||
                fileDto.FileExtension == FileExtensions.Log ||
                fileDto.FileExtension == FileExtensions.Md ||
                fileDto.FileExtension == FileExtensions.Ps1 ||
                fileDto.FileExtension == FileExtensions.Py ||
                fileDto.FileExtension == FileExtensions.Scss ||
                fileDto.FileExtension == FileExtensions.Sql ||
                fileDto.FileExtension == FileExtensions.Ts ||
                fileDto.FileExtension == FileExtensions.Tsx ||
                fileDto.FileExtension == FileExtensions.Txt ||
                fileDto.FileExtension == FileExtensions.Xml ||
                fileDto.FileExtension == FileExtensions.Yaml ||
                fileDto.FileExtension == FileExtensions.Yml)
            {
                dto = _commonFileService.ExtractText(fileDto.Content, fileDto.FileName);
            }
            else if (fileDto.FileExtension == FileExtensions.Csv ||
                fileDto.FileExtension == FileExtensions.Xls ||
                fileDto.FileExtension == FileExtensions.Xlsm ||
                fileDto.FileExtension == FileExtensions.Xlsx)
            {
                dto = _excelService.ExtractText(fileDto.Content, fileDto.FileName);
            }
            else if (fileDto.FileExtension == FileExtensions.Pdf ||
                     fileDto.FileExtension == FileExtensions.Pptx)
            {
                var analyzeResult = await _documentIntelligenceService.ReadAsync(fileDto.Content, cancellationToken);
                dto = [.. analyzeResult.Pages.Select(page => new DocumentExtractorDto
                    {
                        PageNumber = page.PageNumber,
                        PageText = string.Join("\n", page.Lines.Select(line => line.Content))
                    })];
            }
            else if (fileDto.FileExtension == FileExtensions.Doc ||
                     fileDto.FileExtension == FileExtensions.Docx)
            {
                dto = _wordFileService.ExtractText(fileDto.Content, fileDto.FileName);
            }
            else
            {
                throw new InvalidOperationException("Extension not supported for text extraction.");
            }

            return dto;
        }
        #endregion
    }
}
