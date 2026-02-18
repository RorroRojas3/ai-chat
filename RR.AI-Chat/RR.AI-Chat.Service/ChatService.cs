using Aspose.Pdf.AI;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.CallRecords;
using RR.AI_Chat.Common.Enums;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Actions.Chat;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using System.Runtime.CompilerServices;
using System.Text;
using Chat = RR.AI_Chat.Entity.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

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
        IValidator<CreateChatActionDto> createChatValidator,
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
        private readonly IValidator<CreateChatActionDto> _createChatValidator = createChatValidator;
        private readonly IValidator<CreateChatStreamActionDto> _createChatStreamActionValidator = createChatStreamActionValidator;
        private readonly AIChatDbContext _ctx = ctx;

        private readonly string _defaultSystemPrompt = @"
            You are an advanced AI assistant with comprehensive analytical capabilities and access to a powerful suite of specialized tools. Your primary mission is to provide thorough, insightful, and actionable responses that leverage all available resources to deliver maximum value.

            **CRITICAL: ALL responses must be formatted in Markdown. Use proper Markdown syntax for headings, lists, code blocks, tables, links, emphasis, and other formatting elements to ensure clear, well-structured, and readable output.**

            ## CORE CAPABILITIES & TOOLS AVAILABLE:
            
            ### Document Intelligence & Analysis
            - **Document Discovery**: Automatically identify and catalog all documents within the current session
            - **Content Extraction**: Access and retrieve complete document content for comprehensive analysis
            - **Semantic Search**: Perform intelligent searches within documents using advanced vector-based similarity matching
            - **Comparative Analysis**: Conduct detailed side-by-side document comparisons with structured insights
            - **Cross-Reference Analysis**: Identify connections, patterns, and relationships across multiple documents

            ### Advanced Processing & Analysis
            - **Code Execution**: Run computational analysis, data processing, and algorithmic solutions
            - **Data Visualization**: Generate charts, graphs, interactive tables, and visual representations
            - **Statistical Analysis**: Perform quantitative analysis with detailed statistical insights
            - **Pattern Recognition**: Identify trends, anomalies, and relationships in data
            - **Predictive Modeling**: Where applicable, provide forecasting and trend analysis

            ### Content Creation & Enhancement
            - **Image Generation**: Create, edit, and enhance images to illustrate concepts and ideas
            - **Structured Documentation**: Generate comprehensive reports, summaries, and formatted content
            - **Multi-format Output**: Deliver information in various formats (tables, lists, diagrams, etc.)

            ### Contextual Intelligence
            - **Session Awareness**: Maintain full context of ongoing conversations and document interactions
            - **Temporal Context**: Access and utilize locale-specific time information for relevant suggestions
            - **Memory Integration**: For personalization features, direct users to Settings→Personalization→Memory
            - **Task Management**: Set reminders and organize workflows as needed

            ## OPERATIONAL PRINCIPLES:

            ### Proactive Tool Utilization
            - **Anticipate Needs**: Immediately assess what tools would enhance your response before answering
            - **Multi-Tool Coordination**: Use multiple capabilities in combination for comprehensive analysis
            - **Automatic Enhancement**: Always consider how document analysis, visualization, or computation could enrich your answer
            - **Context-Driven Selection**: Choose tools based on the user's intent, even if not explicitly requested

            ### Response Excellence Standards
            - **Comprehensive Coverage**: Provide thorough, detailed responses that explore all relevant aspects
            - **Evidence-Based Analysis**: Support conclusions with data, examples, and specific evidence from available sources
            - **Structured Presentation**: Organize information logically with clear headings, sections, and formatting using Markdown
            - **Actionable Insights**: Include practical recommendations, next steps, and implementation guidance
            - **Multiple Perspectives**: When appropriate, present different viewpoints or approaches

            ### Interactive Intelligence
            - **Question Enhancement**: Expand on user queries to address related important aspects they may not have considered
            - **Progressive Disclosure**: Provide detailed information while maintaining clarity and readability
            - **Follow-up Suggestions**: Recommend additional analyses, investigations, or actions that could be valuable
            - **Adaptive Communication**: Match the user's expertise level and preferred communication style

            ## DOCUMENT WORKFLOW PROTOCOLS:

            When users mention documents, files, or content analysis:
            1. **Immediate Discovery**: First identify what documents are available in the current session
            2. **Content Assessment**: Determine whether full document review or targeted search is most appropriate
            3. **Comprehensive Analysis**: Provide detailed insights, summaries, and actionable findings
            4. **Cross-Document Intelligence**: When multiple documents exist, look for relationships and comparative insights
            5. **Visualization Opportunities**: Consider how charts, tables, or other visual aids could enhance understanding

            ## QUALITY ASSURANCE:
            - Never provide brief or superficial responses when comprehensive analysis is possible
            - Always explain your reasoning and methodology
            - Provide specific examples and evidence to support your conclusions
            - Include relevant context from the session and available documents
            - Suggest follow-up actions or additional analyses that could be valuable
            - Seamlessly integrate tool outputs without exposing technical implementation details
            - **Ensure all responses use proper Markdown formatting for maximum clarity and professionalism**

            ## RESPONSE PHILOSOPHY:
            Excellence means leveraging every available capability to provide the most comprehensive, insightful, and valuable response possible. Don't just answer questions—anticipate needs, provide context, deliver transformative insights, and create responses that exceed expectations. **All responses must be properly formatted in Markdown.**

            Your session identifier is {0}. Use this for maintaining context and accessing session-specific resources throughout our conversation.
            Your user identifier is {1}. Use this for maintaining context and accessing session-specific resources throughout our conversation.

            Operate with invisible mastery: your sophisticated use of these capabilities should enhance every response without ever needing to explicitly mention the tools themselves.
            ";

        public async Task<ChatDto> ChatChatAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = _tokenService.GetOid();

            var chat = await _ctx.Chats
                .AsNoTracking()
                .Where(s => s.Id == id && s.UserId == userId && !s.DateDeactivated.HasValue)
                .Select(s => s.MapToChatDto())
                .FirstOrDefaultAsync(cancellationToken);
            if (chat == null)
            {
                _logger.LogError("Chat with id {Id} not found", id);
                throw new InvalidOperationException($"Chat with id {id} not found");
            }

            return chat;
        }

        public async Task<ChatDto> CreateChasAsync(CreateChatActionDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _createChatValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid();

            if (request.ProjectId.HasValue)
            {
                var projectExists = await _ctx.Projects
                    .Where(p => p.Id == request.ProjectId.Value &&
                            p.UserId == userId &&
                            !p.DateDeactivated.HasValue)
                    .AnyAsync(cancellationToken);
                if (!projectExists)
                {
                    _logger.LogError("Project not found.");
                    throw new InvalidOperationException($"Project with id {request.ProjectId.Value} not found");
                }
            }

            var transaction = await _ctx.Database.BeginTransactionAsync(cancellationToken);
            var date = DateTimeOffset.UtcNow;
            var newChat = new Chat()
            {
                Name = "New Chat",
                UserId = userId,
                DateCreated = date,
                DateModified = date
            };
            await _ctx.AddAsync(newChat, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            var prompt = string.Format(_defaultSystemPrompt, newChat.Id, userId);
            var newCosmosChat = new CosmosChat()
            {
                Id = newChat.Id,
                UserId = userId,
                ProjectId = request.ProjectId,
                Name = newChat.Name,
                TotalTokens = 0,
                DateCreated = date,
                DateModified = date,
                Documents = [],
                Conversations = [new()
                {
                    Id = Guid.NewGuid(),
                    Role = ChatRoles.System,
                    Content = prompt,
                    DateCreated = date,
                }]
            };
            await _ctx.SaveChangesAsync(cancellationToken);

            await _cosmosService.CreateItemAsync(newChat, userId.ToString(), cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return newChat.MapToChatDto();
        }

        public async Task DeactivateChatAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = _tokenService.GetOid();
            var date = DateTimeOffset.UtcNow;

            var chatExists = await _ctx.Chats
                .Where(x => x.Id == id && x.UserId == userId && !x.DateDeactivated.HasValue)
                .AnyAsync(cancellationToken);

            if (!chatExists)
            {
                _logger.LogWarning("Chat with id {Id} not found or already deactivated", id);
                return;
            }

            var chat = await _cosmosService.GetItemAsync<CosmosChat>(id.ToString(), userId.ToString(), cancellationToken);
            if (chat != null)
            {
                chat.DateDeactivated = date;
                await _cosmosService.UpdateItemAsync(chat, chat.Id.ToString(), userId.ToString(), cancellationToken);
            }

            await _ctx.ChatDocumentPages
                .Where(p => p.ChatDocument.ChatId == id && !p.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.ChatDocuments
                .Where(d => d.ChatId == id && !d.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(d => d
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.Sessions
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.DateDeactivated, date)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);
        }

        public async Task DeactivateChatBulkAsync(DeactivateChatBulkActionDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _deactivateChatBulkValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid();`
            var date = DateTimeOffset.UtcNow;

            var sessionIds = await _ctx.Sessions
                .Where(x => request.SessionIds.Contains(x.Id) && x.UserId == userId && !x.DateDeactivated.HasValue)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (sessionIds.Count == 0)
            {
                _logger.LogWarning("No valid sessions found to deactivate.");
                return;
            }

            // Deactivate all pages for documents in these sessions
            var sessionIdsInClause = string.Join(", ", sessionIds.Select(id => $"'{id}'"));
            var cosmosQuery =
                $"SELECT * FROM c WHERE c.id IN ({sessionIdsInClause}) " +
                $"AND c.UserId = '{userId}' AND IS_NULL(c.DateDeactivated)";
            var chats = await _cosmosService.GetItemsAsync<CosmosChat>(cosmosQuery);
            foreach (var chat in chats)
            {
                chat.DateDeactivated = date;
                await _cosmosService.UpdateItemAsync(chat, chat.Id.ToString(), userId.ToString(), cancellationToken);
            }

            await _ctx.SessionDocumentPages
                .Where(p => sessionIds.Contains(p.SessionDocument.SessionId) && !p.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.SessionDocuments
                .Where(d => sessionIds.Contains(d.SessionId) && !d.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(d => d
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.Sessions
                .Where(s => sessionIds.Contains(s.Id))
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.DateDeactivated, date)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);
        }

        public async Task UpdateChatNameAsync(Guid id, CreateChatStreamActionDto request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _createChatStreamActionValidator.ValidateAndThrow(request);

            var chat = await _ctx.Sessions.FindAsync([id], cancellationToken);
            if (chat == null)
            {
                _logger.LogError("Chat with id {Id} not found", id);
                throw new KeyNotFoundException($"Chat with id {id} not found");
            }

            var cosmosChat = await _cosmosService.GetItemAsync<CosmosChat>(id.ToString(), chat.UserId.ToString(), cancellationToken);
            if (cosmosChat == null)
            {
                _logger.LogError("Chat for id {Id} not found in Cosmos DB", id);
                throw new KeyNotFoundException($"Chat for id {id} not found");
            }

            var modelName = await _ctx.Models
                        .AsNoTracking()
                        .Where(x => x.Id == request.ModelId && !x.DateDeactivated.HasValue)
                        .Select(x => x.Name)
                        .FirstOrDefaultAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(modelName))
            {
                _logger.LogError("Model with id {id} not found", request.ModelId);
                throw new InvalidOperationException($"Model with id {request.ModelId} not found");
            }

            var response = await _azureAIFoundry.GetResponseAsync([
                                 new ChatMessage(ChatRole.System, _defaultSystemPrompt),
                                 new ChatMessage(ChatRole.User, $"Create a session name based on the following prompt, please make it 25 maximum and make it a string. Do not have the name on the session nor the id. Just the name based on the prompt. The result must be a string, not markdown. Prompt: {request.Prompt}")
                             ], new() { ModelId = modelName }, cancellationToken);
            if (response == null)
            {
                _logger.LogError("Failed to create name for chat id {Id}", id);
                throw new InvalidOperationException($"Failed to create name for id {id}");
            }

            var name = response.Messages.Last().Text?.Trim() ?? string.Empty;
            chat.Name = name;
            chat.DateModified = DateTimeOffset.UtcNow;
            await _ctx.SaveChangesAsync(cancellationToken);

            await _cosmosService.UpdateItemAsync(chat, chat.Id.ToString(), chat.UserId.ToString(), cancellationToken);
        }

        public async Task<PaginatedResponseDto<ChatDto>> SearchChatsAsync(string? name, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
        {
            var userId = _tokenService.GetOid();

            var query = _ctx.Chats
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.DateDeactivated.HasValue);

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.Name) && EF.Functions.Like(x.Name, $"%{name}%"));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.DateCreated)
                .Skip(skip)
                .Take(take)
                .Select(s => s.MapToChatDto())
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<ChatDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = take,
                CurrentPage = (skip / take) + 1
            };
        }

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
                _logger.LogWarning("Session {SessionId} is already being processed.", sessionId);
                throw new InvalidOperationException($"Session {sessionId} is currently being processed. Please wait for the current request to complete.");
            }

            var userId = _tokenService.GetOid();
            using (lockReleaser)
            {
                var session = await _ctx.Sessions
                                .AsNoTracking()
                                .SingleOrDefaultAsync(x => x.Id == sessionId && 
                                    x.UserId == userId && 
                                    !x.DateDeactivated.HasValue, cancellationToken);
                if (session == null)
                {
                    _logger.LogError("Session with id {Id} not found.", sessionId);
                    throw new KeyNotFoundException($"Session with id {sessionId} not found.");
                }

                var chat = await _cosmosService.GetItemAsync<CosmosChat>(sessionId.ToString(), userId.ToString(), cancellationToken);
                if (chat == null)
                {
                    _logger.LogError("Chat with session id {Id} not found.", sessionId);
                    throw new KeyNotFoundException($"Chat with session id {sessionId} not found.");
                }

                if (chat.Conversations.Count == 1)
                {
                    _ = await _sessionService.CreateSessionNameAsync(sessionId, request, cancellationToken);
                }

                var conversations = new List<ChatMessage>(chat.Conversations.Select(x => new ChatMessage(MappingService.MapToChatRole(x.Role), x.Content)))
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
                
                await _ctx.Sessions
                    .Where(s => s.Id == sessionId)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.InputTokens, x => x.InputTokens + totalInputTokens)
                        .SetProperty(x => x.OutputTokens, x => x.OutputTokens + totalOutputTokens)
                        .SetProperty(x => x.DateModified, date),
                        cancellationToken);

                chat = await _cosmosService.GetItemAsync<CosmosChat>(sessionId.ToString(), userId.ToString(), cancellationToken);
                if (chat != null)
                {
                    chat.DateModified = date;   
                    chat.TotalTokens = chat.TotalTokens + totalInputTokens + totalOutputTokens;
                    chat.Conversations.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Content = request.Prompt,
                        DateCreated = date,
                        Model = model.Name,
                        Role = ChatRoles.User,
                        Tokens = 0,
                    });
                    chat.Conversations.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Content = sb.ToString(),
                        DateCreated = date,
                        Model = model.Name,
                        Role = ChatRoles.Assistant,
                        Usage = new CosmosChatUsage
                        {
                            InputTokens = totalInputTokens,
                            OutputTokens = totalOutputTokens
                        }
                    });
                }
                

                await _cosmosService.UpdateItemAsync(chat, sessionId.ToString(), userId.ToString(), cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid();
            var chat = await _cosmosService.GetItemAsync<CosmosChat>(sessionId.ToString(), userId.ToString(), cancellationToken);
            if (chat == null)
            {
                _logger.LogError("Chat with id {Id} not found.", sessionId);
                throw new KeyNotFoundException($"Chat with id {sessionId} not found.");
            }

            var messages = chat.Conversations
                            .Where(x => x.Role != ChatRoles.System)
                            .Select(x => new SessionMessageDto()
                            {
                                Text = x.Content ?? string.Empty,
                                Role = x.Role
                            })
                            .ToList() ?? [];

            await Task.CompletedTask;
            return new() 
            { 
                Id = sessionId, 
                Name = chat.Name, 
                DateCreated = chat.DateCreated,
                DateModified = chat.DateModified,
                Messages = messages
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
