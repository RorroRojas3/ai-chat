using Aspose.Pdf;
using Microsoft.Extensions.Logging;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IPdfService
    {
        byte[]? GeneratePdfFromHtml(string htmlContent);
    }

    public class PdfService(ILogger<PdfService> logger) : IPdfService
    {
        private readonly ILogger _logger = logger;

        public byte[]? GeneratePdfFromHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                _logger.LogWarning("HTML content is empty. Cannot generate PDF.");
                return null;
            }

            var loadOptions = new HtmlLoadOptions
            {
                PageInfo = new PageInfo
                {
                    Margin = new(0,0,0,0),
                    Width = PageSize.A4.Width,
                    Height = PageSize.A4.Height
                }
            };

            // Convert the HTML string
            var htmlBytes = Encoding.UTF8.GetBytes(htmlContent);
            using var htmlStream = new MemoryStream(htmlBytes);
            using var pdfDoc = new Document(htmlStream, loadOptions);

            using var ms = new MemoryStream();
            pdfDoc.Save(ms, SaveFormat.Pdf);

            return ms.ToArray();
        }

    }
}
