using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace RR.AI_Chat.Service
{
    public interface IDocumentService 
    {
        Task<DocumentDto> CreateDocumentAsync(IFormFile formFile, Guid sessionId);
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
        public async Task<DocumentDto> CreateDocumentAsync(IFormFile formFile, Guid sessionId)
        {
            ArgumentNullException.ThrowIfNull(formFile, nameof(formFile));

            var bytes = await ReadAllBytesAsync(formFile.OpenReadStream());
            var documentExtractors = ExtractTextFromPdfFileAsync(bytes);
            List<DocumentPage> documentPages = [];
            var date = DateTime.UtcNow;

            foreach (var documentExtractor in documentExtractors)
            {
                var embedding = await _embeddingGenerator.GenerateVectorAsync(documentExtractor.PageText);
                documentPages.Add(new DocumentPage 
                { 
                    Number = documentExtractor.PageNumber,
                    Embedding = embedding.ToArray(), 
                    Text = documentExtractor.PageText, 
                    DateCreated = date 
                });
            }

            var document = new Document
            {
                SessionId = sessionId,
                Name = formFile.FileName,
                Extension = GetFileExtension(formFile.FileName),
                Pages = documentPages,
                DateCreated = date
            };

            await _ctx.AddAsync(document);
            await _ctx.SaveChangesAsync();

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
    }
}
