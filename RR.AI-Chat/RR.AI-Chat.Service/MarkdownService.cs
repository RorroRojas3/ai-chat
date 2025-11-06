using Microsoft.Extensions.Logging;
using ReverseMarkdown;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IMarkdownService
    {
        /// <summary>
        /// Generates a Markdown document from the specified HTML content.
        /// </summary>
        /// <param name="htmlContent">
        /// The HTML content to convert. If null or whitespace, the method returns <c>null</c> and a warning is logged.
        /// </param>
        /// <returns>
        /// A UTF-8 encoded byte array containing the converted Markdown; or <c>null</c> if <paramref name="htmlContent"/> is null or whitespace.
        /// </returns>
        /// <remarks>
        /// Conversion uses ReverseMarkdown configured for GitHub-flavored Markdown with smart href handling, pass-through for unknown tags,
        /// comments preserved, and the default code block language set to <c>csharp</c>.
        /// </remarks>
        byte[]? GenerateMarkdownFromHtml(string htmlContent);
    }

    public class MarkdownService(ILogger<MarkdownService> logger) : IMarkdownService
    {
        private readonly ILogger<MarkdownService> _logger = logger;

        /// <inheritdoc />
        public byte[]? GenerateMarkdownFromHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                _logger.LogWarning("HTML content is empty. Cannot generate Markdown.");
                return null;
            }

            var config = new Config
            {
                UnknownTags = Config.UnknownTagsOption.PassThrough,
                GithubFlavored = true,
                SmartHrefHandling = true,
                RemoveComments = false,
                DefaultCodeBlockLanguage = "csharp"
            };

            var converter = new Converter(config);
            var markdown = converter.Convert(htmlContent);

            _logger.LogInformation("Markdown document generated successfully from HTML content.");

            return Encoding.UTF8.GetBytes(markdown);
        }
    }
}
