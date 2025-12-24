using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Repository;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RR.AI_Chat.Entity;
using Microsoft.EntityFrameworkCore;
using RR.AI_Chat.Dto.Actions.Chat;
using FluentValidation;
using RR.AI_Chat.Common.Enums;

namespace RR.AI_Chat.Service
{
    public interface IChatService 
    {
        /// <summary>
        /// Streams chat responses asynchronously for a given session and user prompt.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session.</param>
        /// <param name="request">The chat stream request containing the user prompt, model ID, service ID, and optional MCP servers.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous enumerable of strings representing the streamed chat response chunks.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request validation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the session is already being processed or the session is not found.</exception>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Validates the request and acquires an exclusive lock on the session</description></item>
        /// <item><description>Retrieves the session and validates it belongs to the current user</description></item>
        /// <item><description>Updates the session name if this is the first conversation</description></item>
        /// <item><description>Streams the AI response and tracks token usage</description></item>
        /// <item><description>Updates the session with the new conversation and token counts</description></item>
        /// </list>
        /// </remarks>
        IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, CreateChatStreamActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the conversation history for a specific chat session.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the session conversation details including all messages.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the session is not found or does not belong to the current user.</exception>
        /// <remarks>
        /// This method retrieves all non-system messages from the session and maps them to DTOs.
        /// System messages are excluded from the returned conversation history.
        /// </remarks>
        Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId, CancellationToken cancellationToken);

        /// <summary>
        /// Checks whether a session is currently locked and being processed.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to check.</param>
        /// <returns><c>true</c> if the session is currently locked and busy; otherwise, <c>false</c>.</returns>
        bool IsSessionBusy(Guid sessionId);
    }

    public class ChatService(ILogger<ChatService> logger,
        IDocumentToolService documentToolService,
        ISessionService sessionService,
        IModelService modelService,
        [FromKeyedServices("azureaifoundry")] IChatClient azureAIFoundry,
        IMcpServerService mcpServerService,
        ISessionLockService sessionLockService,
        ITokenService tokenService,
        IAzureCosmosService cosmosService,
        IValidator<CreateChatStreamActionDto> createChatStreamActionValidator,
        AIChatDbContext ctx) : IChatService
    {
        private readonly ILogger _logger = logger;
        private readonly IChatClient _azureAIFoundry = azureAIFoundry;    
        private readonly IDocumentToolService _documentToolService = documentToolService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly IModelService _modelService = modelService;
        private readonly IMcpServerService _mcpServerService = mcpServerService;
        private readonly ISessionLockService _sessionLockService = sessionLockService;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IAzureCosmosService _cosmosService = cosmosService;
        private readonly IValidator<CreateChatStreamActionDto> _createChatStreamActionValidator = createChatStreamActionValidator;
        private readonly AIChatDbContext _ctx = ctx;

        /// <inheritdoc />
        public bool IsSessionBusy(Guid sessionId) => _sessionLockService.IsSessionBusy(sessionId);

        /// <inheritdoc />
        public async IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, CreateChatStreamActionDto request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            _createChatStreamActionValidator.ValidateAndThrow(request);

            var lockReleaser = await _sessionLockService.TryAcquireLockAsync(sessionId, cancellationToken);
            if (lockReleaser == null)
            {
                _logger.LogWarning("Session {sessionId} is already being processed.", sessionId);
                throw new InvalidOperationException($"Session {sessionId} is currently being processed. Please wait for the current request to complete.");
            }

            var userId = _tokenService.GetOid()!.Value;
            using (lockReleaser)
            {
                var session = await _ctx.Sessions
                                .AsNoTracking()
                                .SingleOrDefaultAsync(x => x.Id == sessionId && 
                                    x.UserId == userId && 
                                    !x.DateDeactivated.HasValue, cancellationToken);
                if (session == null || session.Chat == null)
                {
                    _logger.LogError("Session with id {id} not found.", sessionId);
                    throw new InvalidOperationException($"Session with id {sessionId} not found.");
                }

                if (session.Chat.Conversations.Count == 1)
                {
                    _ = await _sessionService.CreateSessionNameAsync(sessionId, request, cancellationToken);
                }

                var conversations = new List<ChatMessage>(session.Chat.Conversations.Select(x => new ChatMessage(MappingService.MapToChatRole(x.Role), x.Content)))
                {
                    new(ChatRole.User, request.Prompt)
                };

                var model = await _modelService.GetModelAsync(request.ModelId, request.ServiceId, cancellationToken);
                var chatClient = _azureAIFoundry;
                var chatOptions = await CreateChatOptions(sessionId, model, request.McpServers, cancellationToken).ConfigureAwait(false);
                StringBuilder sb = new();
                long totalInputTokens = 0, totalOutputTokens = 0;

                await foreach (var message in chatClient.GetStreamingResponseAsync(conversations, chatOptions, cancellationToken))
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

                var date = DateTimeOffset.UtcNow;
                var currentChat = await _ctx.Sessions.Where(s => s.Id == sessionId)
                                    .Select(s => s.Chat!)
                                    .SingleAsync(cancellationToken);
                currentChat.Conversations.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Content = sb.ToString(),
                    DateCreated = date,
                    Model = model.Name,
                    Role = ChatRoles.Assistant,
                    Tokens = totalInputTokens + totalOutputTokens,
                    Usage = new ChatUsage
                    {
                        InputTokens = totalInputTokens,
                        OutputTokens = totalOutputTokens
                    }
                });

                await _ctx.Sessions
                    .Where(s => s.Id == sessionId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.Chat, currentChat)
                        .SetProperty(x => x.InputTokens, x => x.InputTokens + totalInputTokens)
                        .SetProperty(x => x.OutputTokens, x => x.OutputTokens + totalOutputTokens)
                        .SetProperty(x => x.DateModified, date),
                        cancellationToken);

                await _cosmosService.UpdateItemAsync(currentChat, sessionId.ToString(), userId.ToString());
            }
        }

        /// <inheritdoc />
        public async Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid()!.Value;
            var session = await _ctx.Sessions.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId, cancellationToken);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found.", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found.");
            }

            var messages = session.Chat!.Conversations
                            .Where(x => x.Role != ChatRoles.System)
                            .Select(x => new SessionMessageDto() 
                            { 
                                Text = x.Content ?? string.Empty,
                                Role = x.Role == ChatRoles.User ? ChatRoles.User : ChatRoles.System
                            })
                            .ToList();
            if (messages == null || messages.Count == 0)
            {
                _logger.LogError("Session with id {id} does not contain any messages.", sessionId);
            }

            await Task.CompletedTask;
            return new() 
            { 
                Id = sessionId, 
                Name = session.Name!, 
                DateCreated = session.DateCreated,
                DateModified = session.DateModified,
                Messages = messages! 
            };
        }

        /// <summary>
        /// Creates chat options configured with the specified model and tools.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session.</param>
        /// <param name="model">The AI model to use for the chat.</param>
        /// <param name="mcps">A list of MCP server configurations to enable for tool calling.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the configured chat options.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is null.</exception>
        /// <remarks>
        /// If the model has tools enabled, this method will:
        /// <list type="number">
        /// <item><description>Add document tools from the document tool service</description></item>
        /// <item><description>Create MCP clients for each configured MCP server and retrieve their tools</description></item>
        /// <item><description>Configure the chat options to allow multiple tool calls</description></item>
        /// </list>
        /// </remarks>
        private async Task<ChatOptions> CreateChatOptions(Guid sessionId, ModelDto model, List<McpDto> mcps, CancellationToken cancellationToken)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null.");
            }

            ChatOptions chatOptions = new()
            {
                ModelId = model.Name,
                ConversationId = sessionId.ToString() 
            };
            if (model.IsToolEnabled)
            {
                List<AITool> tools = [];
                var documentTools = _documentToolService.GetTools();
                tools.AddRange(documentTools);

                if (mcps.Count > 0)
                {
                    var mcpToolTasks = mcps.Select(async mcp =>
                    {
                        var mcpClient = await _mcpServerService.CreateClientAsync(mcp.Name, cancellationToken);
                        return await _mcpServerService.GetToolsFromServerAsync(mcpClient, cancellationToken);
                    });

                    var mcpToolResults = await Task.WhenAll(mcpToolTasks);
                    tools.AddRange(mcpToolResults.SelectMany(t => t));
                }

                chatOptions.Tools = tools;
                chatOptions.AllowMultipleToolCalls = true;
            }
            
            return chatOptions;
        }
    }
}
