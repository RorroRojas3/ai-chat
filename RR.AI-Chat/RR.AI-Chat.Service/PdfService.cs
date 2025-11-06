using Aspose.Pdf;
using Microsoft.Extensions.Logging;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IPdfService
    {
        /// <summary>
        /// Generates a PDF document from the provided HTML markup using Aspose.Pdf.
        /// </summary>
        /// <param name="htmlContent">The HTML content to render into the PDF. Must be UTF-8 compatible.</param>
        /// <returns>
        /// A byte array containing the generated PDF; null if <paramref name="htmlContent"/> is null, empty, or whitespace.
        /// </returns>
        /// <remarks>
        /// - Renders onto A4-sized pages with zero margins.
        /// - Loads HTML via <see cref="HtmlLoadOptions"/> and encodes the input as UTF-8.
        /// - Logs a warning and returns null when the input is empty or whitespace.
        /// </remarks>
        byte[]? GeneratePdfFromHtml(string htmlContent);
    }

    public class PdfService(ILogger<PdfService> logger) : IPdfService
    {
        private readonly ILogger _logger = logger;

        /// <inheritdoc />
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
                    Margin = new(0, 0, 0, 0),
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
