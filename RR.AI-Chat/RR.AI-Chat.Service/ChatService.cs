using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Repository;
using System.Runtime.CompilerServices;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IChatService 
    {
        IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, CancellationToken cancellationToken);

        Task<ChatCompletionDto> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto request, CancellationToken cancellationToken);

        Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId);
    }

    public class ChatService(ILogger<ChatService> logger, 
        IChatClient chatClient,
        IDocumentToolService documentToolService,
        IHttpContextAccessor httpContextAccessor,
        ISessionService sessionService,
        ChatStore chatStore, AIChatDbContext ctx) : IChatService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IChatClient _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        private readonly IDocumentToolService _documentToolService = documentToolService ?? throw new ArgumentNullException(nameof(documentToolService));
        private readonly ISessionService _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        private readonly ChatStore _chatStore = chatStore;
        private readonly AIChatDbContext _ctx = ctx;

        /// <summary>
        /// Streams chat responses asynchronously for a given session and user prompt.
        /// This method processes the user's message, sends it to the AI chat client, and yields streaming responses in real-time.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session to retrieve and update.</param>
        /// <param name="request">The chat streaming request containing the user's prompt and model configuration.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the streaming operation.</param>
        /// <returns>
        /// An asynchronous enumerable that yields individual text chunks from the AI response as they become available.
        /// Each yielded string represents a portion of the complete AI response.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the session with the specified <paramref name="sessionId"/> is not found in the chat store.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
        /// </exception>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item>Validates that the session exists in the chat store</item>
        /// <item>Creates a session name if this is the second message (after the initial system message)</item>
        /// <item>Adds the user's prompt to the session message history</item>
        /// <item>Streams the AI response in real-time, yielding each text chunk as it arrives</item>
        /// <item>Accumulates the complete response and adds it to the session message history</item>
        /// </list>
        /// The method uses the "gpt-4.1-nano" model and includes document tools for enhanced AI capabilities.
        /// Side effects include modifying the session's message history and potentially updating the session name.
        /// </remarks>
        public async IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var session = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId) ?? throw new InvalidOperationException($"Session with id {sessionId} not found.");
            if (session.Messages.Count == 1)
            {
                var sessionName = await _sessionService.CreateSessionNameAsync(sessionId, request);
                session.Name = sessionName;
            }

            session.Messages.Add(new ChatMessage(ChatRole.User, request.Prompt));

            var chatOptions = CreateChatOptions("gpt-4.1-mini-2025-04-14", sessionId);
            StringBuilder sb = new();
            await foreach (var message in _chatClient.GetStreamingResponseAsync(session.Messages ?? [], chatOptions, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.Append(message.Text);
                yield return message.Text;
            }

            session.Messages?.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));
        }

        /// <summary>
        /// Retrieves a complete chat response asynchronously for a given session and user prompt.
        /// This method processes the user's message, sends it to the AI chat client, and returns the complete response with token usage information.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session to retrieve and update.</param>
        /// <param name="prompt">The chat completion request containing the user's prompt.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="ChatCompletionDto"/> 
        /// with the AI response message, session ID, and token usage statistics (input, output, and total token counts).
        /// </returns>
        /// <exception cref="OperationCanceledException">
        /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
        /// </exception>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item>Adds the user's prompt to the session message history with User role</item>
        /// <item>Retrieves all messages from the specified session</item>
        /// <item>Sends the message history to the AI chat client for processing</item>
        /// <item>Adds the AI response to the session message history with System role</item>
        /// <item>Returns the response with token usage statistics</item>
        /// </list>
        /// <para>
        /// Note: This method currently has a potential issue where it adds the AI response with <see cref="ChatRole.System"/> 
        /// instead of <see cref="ChatRole.Assistant"/>, and uses the first message from the response instead of the last.
        /// </para>
        /// <para>
        /// Side effects include modifying the session's message history by adding both the user prompt and AI response.
        /// If the session with the specified ID is not found, the method will silently fail to add messages.
        /// </para>
        /// </remarks>
        public async Task<ChatCompletionDto> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto prompt, CancellationToken cancellationToken)
        {
            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.User, prompt.Prompt));
            
            var messages = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages;
            var response = await _chatClient.GetResponseAsync(messages ?? [], null,cancellationToken);

            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.System, response.Messages[0].Text));
            
            return new() 
            { 
                SessionId = sessionId, 
                Message = response.Messages.Last().Text, 
                InputTokenCount = response?.Usage?.InputTokenCount,
                OutputTokenCount = response?.Usage?.OutputTokenCount,
                TotalTokenCount = response?.Usage?.TotalTokenCount
            };
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
        /// Creates and configures a ChatOptions instance for AI chat interactions.
        /// </summary>
        /// <param name="modelId">The identifier of the AI model to use for chat operations.</param>
        /// <returns>A configured ChatOptions object with document tools, multi-tool calling enabled, and the specified model ID.</returns>
        /// <remarks>
        /// This method configures the chat options with:
        /// - Document tools from the document tool service for enhanced functionality
        /// - Multiple tool calls allowed for complex operations
        /// - RequireAny tool mode to ensure at least one tool is available
        /// - The specified model ID for consistent AI model usage
        /// </remarks>
        private ChatOptions CreateChatOptions(string modelId, Guid sessionId)
        {
            var documentTools = _documentToolService.GetTools();

            var chatOptions = new ChatOptions
            {
                // Assuming AIFunction is a subclass of AITool, you need to cast each function to AITool
                Tools = documentTools,
                AllowMultipleToolCalls = true,
                ToolMode = ChatToolMode.RequireAny,
                ModelId = modelId,
                ConversationId = sessionId.ToString(),
            };

            return chatOptions;
        }
    }
}
