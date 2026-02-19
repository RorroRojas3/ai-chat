using Markdig;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Common.Enums;
using RR.AI_Chat.Entity;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IHtmlService
    {
        /// <summary>
        /// Generates an HTML document that represents the provided conversation history.
        /// </summary>
        /// <param name="conversations">
        /// The ordered list of conversation messages to render. Only messages with roles
        /// <see cref="ChatRoles.User"/> and <see cref="ChatRoles.Assistant"/> are included. Message
        /// content may contain Markdown, which is converted to HTML.
        /// </param>
        /// <returns>
        /// A string containing the final HTML with the rendered messages and the current UTC timestamp,
        /// or <c>null</c> if <paramref name="conversations"/> is <c>null</c> or empty.
        /// </returns>
        string? GenerateConversationHistoryAsync(List<CosmosConversationMessage> conversations);
    }

    public class HtmlService(ILogger<HtmlService> logger) : IHtmlService
    {
        private readonly ILogger<HtmlService> _logger = logger;
        private readonly string _conversationHistoryTemplate = GetConversationHistoryHTMLContent();
        private const string _userMessage = @"<div class=""message user"">
                                                <div class=""message-header"">User</div>
                                                <div class=""message-content"">
                                                   {{MESSAGE}}
                                                </div>
                                            </div>";
        private const string _assistantMessage = @"<div class=""message assistant"">
                                                    <div class=""message-header"">Assistant</div>
                                                    <div class=""message-content"">
                                                        {{MESSAGE}}
                                                    </div>
                                                </div>";

        /// <inheritdoc />
        public string? GenerateConversationHistoryAsync(List<CosmosConversationMessage> conversations)
        {
            if (conversations == null || conversations.Count == 0)
            {
                _logger.LogWarning("No conversations provided for HTML generation.");
                return null;
            }

            var sb = new StringBuilder();
            foreach (var convo in conversations)
            {
                if (convo.Role == ChatRoles.User)
                {
                    sb.AppendLine(GetUserMessageHTMLContent(convo.Content));
                }
                else if (convo.Role == ChatRoles.Assistant)
                {
                    sb.AppendLine(GetAssistantMessageHTMLContent(convo.Content));
                }
            }
            var messages = sb.ToString();
            var finalHtml = _conversationHistoryTemplate
                            .Replace("{{DATE}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"))
                            .Replace("{{MESSAGES}}", messages);

            return finalHtml;
        }

        /// <summary>
        /// Builds the HTML block for a user message by converting Markdown content to HTML
        /// and embedding it into the user message template.
        /// </summary>
        /// <param name="message">The raw message text, which may contain Markdown.</param>
        /// <returns>The HTML string representing the user message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Conversion is performed using Markdig's <see cref="Markdown.ToHtml(string)"/> without additional sanitization.
        /// Ensure the rendered HTML is used in a trusted context or sanitized as appropriate.
        /// </remarks>
        private string GetUserMessageHTMLContent(string message)
        {
            return _userMessage.Replace("{{MESSAGE}}", Markdown.ToHtml(message));
        }

        /// <summary>
        /// Builds the HTML block for an assistant message by converting Markdown content to HTML
        /// and embedding it into the assistant message template.
        /// </summary>
        /// <param name="message">The raw message text, which may contain Markdown.</param>
        /// <returns>The HTML string representing the assistant message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is <c>null</c>.</exception>
        /// <remarks>
        /// Conversion is performed using Markdig's <see cref="Markdown.ToHtml(string)"/> without additional sanitization.
        /// Ensure the rendered HTML is used in a trusted context or sanitized as appropriate.
        /// </remarks>
        private string GetAssistantMessageHTMLContent(string message)
        {
            return _assistantMessage.Replace("{{MESSAGE}}", Markdown.ToHtml(message));
        }

        /// <summary>
        /// Loads the HTML template used to render the conversation history document.
        /// </summary>
        /// <returns>
        /// The raw HTML template content read from the file
        /// <c>Files/conversation-history.html</c> located under the application's base directory.
        /// </returns>
        /// <exception cref="FileNotFoundException">Thrown if the template file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the template directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the template file is denied.</exception>
        /// <exception cref="IOException">Thrown if an I/O error occurs while reading the file.</exception>
        /// <remarks>
        /// The base directory is resolved from <see cref="AppContext.BaseDirectory"/>.
        /// </remarks>
        private static string GetConversationHistoryHTMLContent()
        {
            var baseDir = AppContext.BaseDirectory; // where the .dll runs from
            var path = Path.Combine(baseDir, "Files", "conversation-history.html");
            return File.ReadAllText(path);
        }
    }
}
