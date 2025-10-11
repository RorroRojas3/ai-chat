using Hangfire.Server;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace RR.AI_Chat.Service
{
    public interface IDocumentService 
    {
        Task<DocumentDto> CreateDocumentAsync(PerformContext? context, FileDataDto fileDataDto, Guid sessionId, CancellationToken cancellationToken);
    }

    public class DocumentService(ILogger<DocumentService> logger, 
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        AIChatDbContext ctx) : IDocumentService
    {
        private readonly ILogger _logger = logger;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator = embeddingGenerator;
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
        public async Task<DocumentDto> CreateDocumentAsync(PerformContext? context, FileDataDto fileDataDto, Guid sessionId, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));
            ArgumentNullException.ThrowIfNull(fileDataDto, nameof(fileDataDto));

            var jobId = context.BackgroundJob.Id;
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Queued.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 0);
            _logger.LogInformation("Starting document creation. Job ID: {JobId}, Session ID: {SessionId}, File Name: {FileName}", jobId, sessionId, fileDataDto.FileName);


            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Reading.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 25);

            var documentExtractors = ExtractTextFromPdfFileAsync(fileDataDto.Content);
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Extracting.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 50);

            List<DocumentPage> documentPages = [];
            var date = DateTime.UtcNow;

            var tasks = new List<Task<(int PageNumber, ReadOnlyMemory<float> Embedding, string PageText)>>();

            foreach (var documentExtractor in documentExtractors)
            {
                var task = GenerateEmbeddingForPageAsync(documentExtractor, cancellationToken);
                tasks.Add(task);

                // Process in batches of 10
                if (tasks.Count == 10)
                {
                    var completedTasks = await Task.WhenAll(tasks);
                    foreach (var result in completedTasks)
                    {
                        documentPages.Add(new DocumentPage
                        {
                            Number = result.PageNumber,
                            Embedding = result.Embedding.ToArray(),
                            Text = result.PageText,
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
                        Number = result.PageNumber,
                        Embedding = result.Embedding.ToArray(),
                        Text = result.PageText,
                        DateCreated = date
                    });
                }
            }
            context.SetJobParameter(JobName.Status.ToString(), JobStatus.Embedding.ToString());
            context.SetJobParameter(JobName.Progress.ToString(), 75);

            var document = new Document
            {
                SessionId = sessionId,
                Name = fileDataDto.FileName,
                Extension = GetFileExtension(fileDataDto.FileName),
                Pages = documentPages,
                DateCreated = date
            };

            await _ctx.AddAsync(document);
            await _ctx.SaveChangesAsync();

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
        public static List<DocumentExtractorDto> ExtractTextFromPdfFileAsync(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes, nameof(bytes));

            // Load PDF from byte array
            using MemoryStream stream = new(bytes);

            var dto = new List<DocumentExtractorDto>();

            // Open the PDF document
            using PdfDocument document = PdfDocument.Open(stream);
            // Iterate through all pages
            for (int i = 0; i < document.NumberOfPages; i++)
            {
                // Get the page (note: PdfPig uses 1-based page numbering)
                Page page = document.GetPage(i + 1);

                // Extract text from the page
                string pageText = page.Text;

                // Add the text to our StringBuilder
                dto.Add(new() { PageNumber = i + 1, PageText = pageText});
            }

            return dto;
        }

        // Add this helper method to the DocumentService class
        private async Task<(int PageNumber, ReadOnlyMemory<float> Embedding, string PageText)> GenerateEmbeddingForPageAsync(DocumentExtractorDto documentExtractor, CancellationToken cancellationToken)
        {
            var embedding = await _embeddingGenerator.GenerateVectorAsync(string.IsNullOrWhiteSpace(documentExtractor.PageText) ? "EMPTY PAGE" : documentExtractor.PageText, null, cancellationToken);
            return (documentExtractor.PageNumber, embedding, documentExtractor.PageText);
        }
    }
}
