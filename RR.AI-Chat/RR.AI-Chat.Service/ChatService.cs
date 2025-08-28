using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Repository;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RR.AI_Chat.Entity;
using Microsoft.EntityFrameworkCore;

namespace RR.AI_Chat.Service
{
    public interface IChatService 
    {
        IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, CancellationToken cancellationToken);

        Task<ChatCompletionDto> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto request, CancellationToken cancellationToken);

        Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId);
    }

    public class ChatService(ILogger<ChatService> logger,
        IDocumentToolService documentToolService,
        ISessionService sessionService,
        IModelService modelService,
        [FromKeyedServices("ollama")] IChatClient ollamaClient,
        [FromKeyedServices("openai")] IChatClient openAiClient,
        [FromKeyedServices("azureaifoundry")] IChatClient azureOpenAiClient,
        [FromKeyedServices("anthropic")] IChatClient anthropic,
        IMcpServerService mcpServerService,
        AIChatDbContext ctx) : IChatService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IChatClient _ollamaClient = ollamaClient ?? throw new ArgumentNullException(nameof(ollamaClient));
        private readonly IChatClient _openAiClient = openAiClient ?? throw new ArgumentNullException(nameof(openAiClient));
        private readonly IChatClient _azureOpenAiClient = azureOpenAiClient ?? throw new ArgumentNullException(nameof(azureOpenAiClient));      
        private readonly IDocumentToolService _documentToolService = documentToolService ?? throw new ArgumentNullException(nameof(documentToolService));
        private readonly ISessionService _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        private readonly IModelService _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        private readonly IChatClient _anthropic = anthropic ?? throw new ArgumentNullException(nameof(anthropic));
        private readonly IMcpServerService _mcpServerService = mcpServerService ?? throw new ArgumentNullException(nameof(mcpServerService));
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
            var session = await _ctx.Sessions.FindAsync(sessionId, cancellationToken);
            if (session == null || session.Conversations == null)
            {
                _logger.LogError("Session with id {id} not found.", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found.");
            }

            if (session.Conversations.Count == 1)
            {
                var sessionName = await _sessionService.CreateSessionNameAsync(sessionId, request);
                session.Name = sessionName;
            }

            session.Conversations.Add(new Conversation(ChatRole.User, request.Prompt));

            var model = await _modelService.GetModelAsync(request.ModelId, request.ServiceId);
            var chatClient = GetChatClient(request.ServiceId);
            var chatOptions = await CreateChatOptions(sessionId, model).ConfigureAwait(false);
            StringBuilder sb = new();
            long totalInputTokens = 0, totalOutputTokens = 0;
            await foreach (var message in chatClient.GetStreamingResponseAsync(session.Conversations.Select(x => new ChatMessage(x.Role, x.Content)) ?? [], chatOptions, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrEmpty(message.Text))
                {
                    sb.Append(message.Text);
                }

                if (message.Contents != null && 
                    message.Contents.Count > 0)
                {
                    // Check for usage content to track token consumption during streaming
                    var usageContent = message.Contents.OfType<UsageContent>().FirstOrDefault();
                    if (usageContent != null)
                    {
                        totalInputTokens += usageContent.Details?.InputTokenCount ?? 0;
                        totalOutputTokens += usageContent.Details?.OutputTokenCount ?? 0;
                    }
                }
                yield return message.Text;
            }

            // Update token counts once at the end
            session.InputTokens += totalInputTokens;
            session.OutputTokens += totalOutputTokens;

            session.Conversations?.Add(new Conversation(ChatRole.Assistant, sb.ToString()));
            session.DateModified = DateTime.UtcNow;
            _ctx.Entry(session).Property(e => e.Conversations).IsModified = true;

            await _ctx.SaveChangesAsync(cancellationToken);
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
            var session = await _ctx.Sessions.FindAsync(sessionId);
            if (session == null || session.Conversations == null)
            {
                _logger.LogError("Session with id {id} not found.", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found.");
            }

            var conversation = new Conversation(ChatRole.User, prompt.Prompt);
            session.Conversations.Add(conversation);

            var response = await _ollamaClient.GetResponseAsync([new ChatMessage(conversation.Role, conversation.Content)], null,cancellationToken);

            var conversationResponse = new Conversation(ChatRole.System, response.Messages.Last().Text);
            session.Conversations.Add(conversationResponse);
            session.DateModified = DateTime.UtcNow;
            _ctx.Entry(session).Property(e => e.Conversations).IsModified = true;
            await _ctx.SaveChangesAsync(cancellationToken);

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
            var session = await _ctx.Sessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sessionId);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found.", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found.");
            }

            var messages = session.Conversations!
                            .Where(x => x.Role != ChatRole.System)
                            .Select(x => new SessionMessageDto() 
                            { 
                                Text = x.Content ?? string.Empty,
                                Role = x.Role == ChatRole.User ? ChatRoleType.User : ChatRoleType.System
                            })
                            .ToList();
            if (messages == null || messages.Count == 0)
            {
                _logger.LogError("Session with id {id} does not contain any messages.", sessionId);
            }

            await Task.CompletedTask;
            return new() { Id = sessionId, Name = session.Name!, Messages = messages! };
        }

        
        /// <summary>
        /// Retrieves the appropriate AI chat client based on the specified service identifier.
        /// This method acts as a factory pattern implementation to resolve the correct chat client instance
        /// from the available AI service providers (Ollama, OpenAI, Azure OpenAI).
        /// </summary>
        /// <param name="serviceId">
        /// The unique identifier of the AI service type. Must correspond to one of the predefined 
        /// service types: <see cref="AIServiceType.Ollama"/>, <see cref="AIServiceType.OpenAI"/>, 
        /// or <see cref="AIServiceType.AzureOpenAI"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IChatClient"/> instance configured for the specified AI service provider.
        /// The returned client is ready to process chat requests using the appropriate service's API.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="serviceId"/> does not match any of the supported 
        /// AI service types. This indicates an unsupported or invalid service configuration.
        /// </exception>
        /// <remarks>
        /// This method provides a centralized way to access different AI chat service clients based on 
        /// runtime configuration. The method supports the following AI service providers:
        /// <list type="bullet">
        /// <item><description><strong>Ollama:</strong> Local AI model service for self-hosted deployments</description></item>
        /// <item><description><strong>OpenAI:</strong> Cloud-based OpenAI GPT models and services</description></item>
        /// <item><description><strong>Azure OpenAI:</strong> Microsoft Azure-hosted OpenAI services</description></item>
        /// </list>
        /// <para>
        /// Each client is injected via dependency injection with a keyed service registration, ensuring
        /// proper isolation and configuration for each AI service provider.
        /// </para>
        /// </remarks>
        private IChatClient GetChatClient(Guid serviceId)
        {
            if (AIServiceType.Ollama == serviceId)
            {
                return _ollamaClient;
            }
            else if (AIServiceType.OpenAI == serviceId)
            {
                return _openAiClient;
            }
            else if (AIServiceType.AzureAIFoundry == serviceId)
            {
                return _azureOpenAiClient;
            }
            else if (AIServiceType.Anthropic == serviceId)
            {
                // Assuming you have an Anthropic client registered similarly
                return _anthropic;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported AI service type: {serviceId}");
            }
        }

        /// <summary>
        /// Creates chat options configured for the specified model and session.
        /// </summary>
        /// <param name="sessionId">The session identifier for conversation tracking.</param>
        /// <param name="model">The model configuration containing name and tool enablement settings.</param>
        /// <returns>A configured <see cref="ChatOptions"/> instance with model-specific settings and optional tools.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        private async Task<ChatOptions> CreateChatOptions(Guid sessionId, ModelDto model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }

            ChatOptions chatOptions = new()
            {
                AllowMultipleToolCalls = true,
                ModelId = model.Name,
                ConversationId = sessionId.ToString(),
                AdditionalProperties = new AdditionalPropertiesDictionary
                {
                    { "max_completion_tokens", 6_000},
                },
                MaxOutputTokens = model.Name.Contains("gpt") ? null : 6_000
            };
            if (model.IsToolEnabled)
            {
                List<AITool> tools = [];
                var documentTools = _documentToolService.GetTools();
                //var mcpTools = await _mcpServerService.GetToolsAsync().ConfigureAwait(false);
                tools.AddRange(documentTools);
                //tools.AddRange(mcpTools);
                chatOptions.Tools = tools;
                chatOptions.AllowMultipleToolCalls = true;
            }
            
            return chatOptions;
        }
    }
}
