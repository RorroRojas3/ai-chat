using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
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

        Task<List<SessionDto>> GetSessionsAsync();

        Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId);
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

            return response.Message.Text ?? string.Empty;
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
            var session = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (session == null)
            {
                throw new InvalidOperationException($"Session with id {sessionId} not found.");
            }

            if (session.Messages.Count == 1)
            {
                var sessionName = await CreateSessionNameAsync(sessionId, request);
                session.Name = sessionName;
            }

            session.Messages.Add(new ChatMessage(ChatRole.User, request.Prompt));


            StringBuilder sb = new();
            await foreach (var message in _chatClient.CompleteStreamingAsync(session.Messages ?? [], new ChatOptions() { ModelId = request.ModelId}))
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.Append(message.Text);
                yield return message.Text;
            }

            session.Messages?.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));
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
            var models = _configuration.GetSection("Models").Get<string[]>();
            if (models == null)
            {
                return [];
            }

            await Task.CompletedTask;
            return [.. models.Select(m => new ModelDto { Name = m })];
        }

        public async Task<List<SessionDto>> GetSessionsAsync()
        {
            var sessions = _chatStore.Sessions.Take(10).Select(s => new SessionDto { Id = s.SessionId, Name = s.Name }).ToList();
            await Task.CompletedTask;
            return sessions;
        }

        /// <summary>
        /// Retrieves the conversation of a specific chat session asynchronously.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the session conversation details.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the session with the specified ID is not found or does not contain any messages.
        /// </exception>
        public async Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId)
        {
            var session = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found.", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found.");
            }

            var messages = session.Messages
                            .Where(x => x.Role != ChatRole.System)
                            .Select(x => new SessionMessageDto() 
                            { 
                                Text = x.Text ?? string.Empty,
                                Role = x.Role == ChatRole.User ? ChatRoleType.User : ChatRoleType.System
                            })
                            .ToList();
            if (messages == null || messages.Count == 0)
            {
                _logger.LogError("Session with id {id} does not contain any messages.", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} does not contain any messages.");
            }

            await Task.CompletedTask;
            return new() { Id = sessionId, Name = session.Name, Messages = messages };
        }

        /// <summary>
        /// Creates a session name asynchronously based on the provided request.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="request">The request containing the prompt and model ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated session name.</returns>
        /// <exception cref="ArgumentException">Thrown when the request is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the session name creation fails.</exception>
        private async Task<string> CreateSessionNameAsync(Guid sessionId, ChatStreamRequestdto request)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(request));

            var response = await _chatClient.CompleteAsync([
                                 new ChatMessage(ChatRole.System, "You are a helpful AI assistant."),
                                 new ChatMessage(ChatRole.User, $"Create a session name based on the following prompt, please make it 50 characters or less and make it a string, not markdown. Prompt: {request.Prompt}")
                             ], new() { ModelId = request.ModelId }, CancellationToken.None);
            if (response == null)
            {
                _logger.LogError("Failed to create session name for session id {id}", sessionId);
                throw new InvalidOperationException($"Failed to create session name for id {sessionId}");
            }

            return response.Message.Text ?? string.Empty;
        }
    }
}
