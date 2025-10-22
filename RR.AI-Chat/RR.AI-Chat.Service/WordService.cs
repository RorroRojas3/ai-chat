using Aspose.Words;
using Aspose.Words.Loading;
using Microsoft.Extensions.Logging;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IWordService
    {
        byte[]? GenerateWordFromHtml(string htmlContent);
    }

    public class WordService(ILogger<WordService> logger) : IWordService
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
    }
}
