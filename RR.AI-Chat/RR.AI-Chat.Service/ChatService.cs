using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using System.Runtime.CompilerServices;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IChatService 
    {
        Task<string> GetChatCompletionAsync(CancellationToken cancellationToken, string question);

        IAsyncEnumerable<string?> GetChatStreamingAsync(CancellationToken cancellationToken, string prompt);

        SessionDto CreateChatSessionAsync();

        IAsyncEnumerable<string?> GetChatStreamingAsync(CancellationToken cancellationToken, Guid sessionId, ChatStreamRequestdto request);

        Task<ChatCompletionDto> GetChatCompletionAsync(CancellationToken cancellationToken, Guid sessionId, ChatCompletionRequestDto request);
    }

    public class ChatService : IChatService
    {
        private ILogger _logger;
        private IChatClient _chatClient;
        private ChatStore _chatStore;

        public ChatService(ILogger<ChatService> logger, IChatClient chatClient, ChatStore chatStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
            _chatStore = chatStore;
        }

        /// <summary>
        /// Gets the chat completion asynchronously based on the provided question.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <param name="question">The question to send to the chat client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the chat response message.</returns>
        public async Task<string> GetChatCompletionAsync(CancellationToken cancellationToken, string question)
        {
            var response = await _chatClient.CompleteAsync([
                new ChatMessage(ChatRole.System, "You are a helpful AI assistant"),
                new ChatMessage(ChatRole.User, question),
            ], null, cancellationToken);

            return response.Message.Text;
        }

        /// <summary>
        /// Streams chat responses asynchronously based on the provided prompt.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <param name="prompt">The prompt to send to the chat client.</param>
        /// <returns>An asynchronous stream of chat response messages.</returns>
        public async IAsyncEnumerable<string?> GetChatStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken, string prompt)
        {
            await foreach (var message in _chatClient.CompleteStreamingAsync(prompt))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return message.Text;
            }
        }

        public SessionDto CreateChatSessionAsync()
        {
            var guid = Guid.NewGuid();

            var chatSession = new ChatSesion
            {
                SessionId = guid,
                Messages =
                [
                    new ChatMessage(ChatRole.System, 
                    "You are a helpful AI assistant which only job is to make sure that Nelly Gonzales " +
                    "says yes to being Rodrigo Rojas's valentine date. You will not take no for an answer. " + 
                    "If she were to say no, make reasons of why she should say yes every time and make it concise. " +
                    "Only when she yes, you will share the details of the date or the video which are that the reversation is set for February 14, 2025 at " +
                    "8:00pm EST at 4600 Roswell Rd suite b150, Atlanta, GA 30342. Please share this." +
                    "The restaurant is called Pendolino, mention that Rodrigo chose that resturant because he know" +
                    "That Nelly's favorite type of cuisine is Italian. Also," +
                    "you will share this youtube link for her to watch: https://www.youtube.com/shorts/ZTBc3zZp0Yc showing" +
                    "how excited Rodrigo is about going")
                ]
            };
            _chatStore.Sessions.Add(chatSession);

            return new() { Id = guid };
        }

        public async IAsyncEnumerable<string?> GetChatStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken, Guid sessionId, ChatStreamRequestdto request)
        {
            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.User, request.Prompt));
            var messages = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages;

            StringBuilder sb = new();
            await foreach (var message in _chatClient.CompleteStreamingAsync(messages))
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.Append(message.Text);
                yield return message.Text;
            }

            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.System, sb.ToString()));
        }

        public async Task<ChatCompletionDto> GetChatCompletionAsync(CancellationToken cancellationToken, Guid sessionId, ChatCompletionRequestDto prompt)
        {
            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.User, prompt.Prompt));
            
            var messages = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages;
            var response = await _chatClient.CompleteAsync(messages, null,cancellationToken);

            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.System, response.Message.Text));
            
            return new() 
            { 
                SessionId = sessionId, 
                Message = response.Message.Text, 
                InputTokenCount = response?.Usage?.InputTokenCount,
                OutputTokenCount = response?.Usage?.OutputTokenCount,
                TotalTokenCount = response?.Usage?.TotalTokenCount
            };
        }
    }
}
