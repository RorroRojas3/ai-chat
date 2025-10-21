using Markdig;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Entity;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IHtmlService
    {
        string? GenerateConversationHistoryAsync(List<Conversation> conversations);
    }

    public class HtmlService : IHtmlService
    {
        private readonly ILogger<HtmlService> _logger;
        private readonly string _conversationHistoryTemplate;
        private readonly string _userMessage = @"<div class=""message user"">
                                                <div class=""message-header"">User</div>
                                                <div class=""message-content"">
                                                   {{MESSAGE}}
                                                </div>
                                            </div>";
        private readonly string _assistantMessage = @"<div class=""message assistant"">
                                                    <div class=""message-header"">Assistant</div>
                                                    <div class=""message-content"">
                                                        {{MESSAGE}}
                                                    </div>
                                                </div>";

        public HtmlService(ILogger<HtmlService> logger)
        {
            _logger = logger;
            _conversationHistoryTemplate = GetConversationHistoryHTMLContent();
        }

        public string? GenerateConversationHistoryAsync(List<Conversation> conversations)
        {
            if (conversations == null || conversations.Count == 0)
            {
                _logger.LogWarning("No conversations provided for HTML generation.");
                return null;
            }

            var sb = new StringBuilder();
            foreach (var convo in conversations)
            {
                if (convo.Role == ChatRole.User)
                {
                    sb.AppendLine(GetUserMessageHTMLContent(convo.Content));
                }
                else if (convo.Role == ChatRole.Assistant)
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

        private string GetUserMessageHTMLContent(string message)
        {
            return _userMessage.Replace("{{MESSAGE}}", Markdown.ToHtml(message));
        }

        private string GetAssistantMessageHTMLContent(string message)
        {
            return _assistantMessage.Replace("{{MESSAGE}}", Markdown.ToHtml(message));
        }

        private static string GetConversationHistoryHTMLContent()
        {
            var baseDir = AppContext.BaseDirectory; // where the .dll runs from
            var path = Path.Combine(baseDir, "Files", "conversation-history.html");
            return File.ReadAllText(path);
        }
    }
}
