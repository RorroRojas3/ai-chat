using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
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

        Task<List<ModelDto>> GetModelsAsync();
    }

    public class ChatService : IChatService
    {
        private readonly ILogger _logger;
        private readonly IChatClient _chatClient;
        private readonly IConfiguration _configuration;
        private readonly ChatStore _chatStore;

        public ChatService(ILogger<ChatService> logger, IChatClient chatClient, IConfiguration configuration, ChatStore chatStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
            _configuration = configuration;
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
                    new ChatMessage(ChatRole.System, "You are a helpful AI assistant.")
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
            var response = await _chatClient.CompleteAsync(messages ?? [], null,cancellationToken);

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

        public async Task<List<ModelDto>> GetModelsAsync()
        {
            var models = _configuration.GetValue<List<string>>("Models");
            if (models == null)
            {
                return [];
            }

            await Task.CompletedTask;
            return models.Select(m => new ModelDto { Name = m }).ToList();
        }
    }
}
