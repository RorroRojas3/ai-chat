using Aspose.Words;
using Aspose.Words.Loading;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Service.Common.Interface;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IWordService
    {
        byte[]? GenerateWordFromHtml(string htmlContent);
    }

    public class WordService(ILogger<WordService> logger) : IWordService, IFileService
    {
        private readonly ILogger<WordService> _logger = logger;

        public byte[]? GenerateWordFromHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                _logger.LogWarning("HTML content is empty. Cannot generate Word document.");
                return null;
            }

            using var memoryStream = new MemoryStream();
            byte[] htmlBytes = Encoding.UTF8.GetBytes(htmlContent);
            using var htmlStream = new MemoryStream(htmlBytes);

            var loadOptions = new LoadOptions
            {
                LoadFormat = LoadFormat.Html
            };
            var document = new Document(htmlStream, loadOptions);

            document.Save(memoryStream, SaveFormat.Docx);

            _logger.LogInformation("Word document generated successfully from HTML content.");

            return memoryStream.ToArray();
        }

        public List<DocumentExtractorDto> ExtractText(byte[] bytes, string fileName)
        {
            ArgumentNullException.ThrowIfNull(bytes);
            ArgumentNullException.ThrowIfNull(fileName);

            using var memoryStream = new MemoryStream(bytes);
            var document = new Document(memoryStream);

            _logger.LogInformation("Starting text extraction from Word document {Name}.", fileName);

            List<DocumentExtractorDto> dto = [];
            var pageCount = document.PageCount;

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                var extractedPage = document.ExtractPages(pageIndex, 1);
                var pageText = extractedPage.ToString(SaveFormat.Text);

                dto.Add(new DocumentExtractorDto
                {
                    PageNumber = pageIndex + 1,
                    PageText = pageText
                });
            }

            _logger.LogInformation("Completed text extraction from Word document {Name}. Total pages: {PageCount}.", fileName, pageCount);
            return dto;
        }
    }
}
