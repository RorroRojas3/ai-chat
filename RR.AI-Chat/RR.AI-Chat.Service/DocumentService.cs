using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Pgvector;
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
                    Embedding = new Vector(embedding), 
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
                Id = document.Id,
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
