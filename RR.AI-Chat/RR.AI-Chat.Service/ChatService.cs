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

        Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId, CancellationToken cancellationToken);

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
        AIChatDbContext ctx) : IChatService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IChatClient _azureAIFoundry = azureAIFoundry ?? throw new ArgumentNullException(nameof(azureAIFoundry));      
        private readonly IDocumentToolService _documentToolService = documentToolService ?? throw new ArgumentNullException(nameof(documentToolService));
        private readonly ISessionService _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        private readonly IModelService _modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        private readonly IMcpServerService _mcpServerService = mcpServerService ?? throw new ArgumentNullException(nameof(mcpServerService));
        private readonly ISessionLockService _sessionLockService = sessionLockService ?? throw new ArgumentNullException(nameof(sessionLockService));
        private readonly ITokenService _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        private readonly AIChatDbContext _ctx = ctx;

        public bool IsSessionBusy(Guid sessionId) => _sessionLockService.IsSessionBusy(sessionId);

        public async IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
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
                                .SingleOrDefaultAsync(x => x.Id == sessionId && 
                                    x.UserId == userId && 
                                    !x.DateDeactivated.HasValue, cancellationToken);
                if (session == null || session.Conversations == null)
                {
                    _logger.LogError("Session with id {id} not found.", sessionId);
                    throw new InvalidOperationException($"Session with id {sessionId} not found.");
                }

                if (session.Conversations.Count == 1)
                {
                    var sessionName = await _sessionService.CreateSessionNameAsync(sessionId, request, cancellationToken);
                    session.Name = sessionName;
                }

                session.Conversations.Add(new Conversation(ChatRole.User, request.Prompt));

                var model = await _modelService.GetModelAsync(request.ModelId, request.ServiceId, cancellationToken);
                var chatClient = _azureAIFoundry;
                var chatOptions = await CreateChatOptions(sessionId, model, cancellationToken).ConfigureAwait(false);
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
        }

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
            return new() 
            { 
                Id = sessionId, 
                Name = session.Name!, 
                DateCreated = session.DateCreated,
                DateModified = session.DateModified,
                Messages = messages! 
            };
        }

        private async Task<ChatOptions> CreateChatOptions(Guid sessionId, ModelDto model, CancellationToken cancellationToken)
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
                var mcpClient = await _mcpServerService.CreateClientAsync("Test MCP", cancellationToken);
                var mcpTools = await _mcpServerService.GetToolsFromServerAsync(mcpClient, cancellationToken);
                tools.AddRange(documentTools);
                tools.AddRange(mcpTools);
                chatOptions.Tools = tools;
                chatOptions.AllowMultipleToolCalls = true;
            }
            
            await Task.CompletedTask;
            return chatOptions;
        }
    }
}
