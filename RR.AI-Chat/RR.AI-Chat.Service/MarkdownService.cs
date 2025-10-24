using Microsoft.Extensions.Logging;
using ReverseMarkdown;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IMarkdownService
    {
        byte[]? GenerateMarkdownFromHtml(string htmlContent);
    }

    public class MarkdownService(ILogger<MarkdownService> logger) : IMarkdownService
    {
        private readonly ILogger<MarkdownService> _logger = logger;

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
