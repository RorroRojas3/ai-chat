using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace RR.AI_Chat.Service
{
    public interface IChatService 
    {
        Task<string> GetChatCompletionAsync(CancellationToken cancellationToken, string question);

        IAsyncEnumerable<string?> GetChatStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken, string prompt);
    }

    public class ChatService : IChatService
    {
        private ILogger _logger;
        private IChatClient _chatClient;

        public ChatService(ILogger<ChatService> logger, IChatClient chatClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        }

        public async Task<string> GetChatCompletionAsync(CancellationToken cancellationToken, string question)
        {
            var response = await _chatClient.CompleteAsync([
                new ChatMessage(ChatRole.System, "You are a helpful AI assistant"),
                new ChatMessage(ChatRole.User, question),
            ], null, cancellationToken);

            return response.Message.Text;
        }

        public async IAsyncEnumerable<string?> GetChatStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken, string prompt)
        {
            await foreach (var message in _chatClient.CompleteStreamingAsync(prompt))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return message.Text;
            }
        }
    }
}
