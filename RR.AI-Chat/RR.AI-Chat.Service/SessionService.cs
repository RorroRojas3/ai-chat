using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using RR.AI_Chat.Common.Enums;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Actions.Chat;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface ISessionService
    {
        /// <summary>
        /// Retrieves a specific chat session asynchronously by its unique identifier.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="SessionDto"/> 
        /// with the session details.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Retrieves the current user ID from the token service</description></item>
        /// <item><description>Queries the database for an active session matching the ID and belonging to the user</description></item>
        /// <item><description>Maps the session entity to a DTO for return</description></item>
        /// </list>
        /// Only active sessions (not deactivated) belonging to the current user can be retrieved.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the session is not found or doesn't belong to the user.</exception>
        Task<SessionDto> GetSessionsAsync(Guid sessionId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new chat session asynchronously.
        /// </summary>
        /// <param name="request">The request containing optional project ID for session association.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="SessionDto"/> 
        /// with the unique identifier of the newly created session.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Validates the request using the create session validator</description></item>
        /// <item><description>Retrieves the current user ID from the token service</description></item>
        /// <item><description>If a project ID is provided, verifies the project exists and belongs to the user</description></item>
        /// <item><description>Creates a new <see cref="Session"/> entity with the current UTC timestamp</description></item>
        /// <item><description>Persists the session to the database</description></item>
        /// <item><description>Initializes the session with a system prompt containing the session and user IDs</description></item>
        /// <item><description>Creates conversation history with the system message</description></item>
        /// </list>
        /// The newly created session is initialized with a system message containing the default assistant prompt
        /// formatted with the session ID and user ID.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request validation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the specified project is not found or doesn't belong to the user.</exception>
        /// <exception cref="DbUpdateException">Thrown when the database update operation fails.</exception>
        Task<SessionDto> CreateChatSessionAsync(CreateSessionActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a session name asynchronously based on the provided request.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="request">The request containing the prompt and model ID.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated session name.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Validates the request using the create chat stream validator</description></item>
        /// <item><description>Retrieves the session from the database</description></item>
        /// <item><description>Retrieves the model name from the database</description></item>
        /// <item><description>Generates a session name using the AI chat client based on the user's prompt</description></item>
        /// <item><description>Updates the session with the generated name and modification date</description></item>
        /// </list>
        /// The generated name is limited to 25 characters maximum and is based on the content of the user's prompt.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request validation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the session or model is not found, or when session name creation fails.</exception>
        Task<string> CreateSessionNameAsync(Guid sessionId, CreateChatStreamActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Searches for sessions asynchronously based on the provided filter, with pagination support.
        /// </summary>
        /// <param name="filter">An optional string to filter sessions by name. If null or whitespace, no filtering is applied.</param>
        /// <param name="skip">The number of sessions to skip for pagination. Defaults to 0.</param>
        /// <param name="take">The number of sessions to take for pagination. Defaults to 10.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="PaginatedResponseDto{SessionDto}"/> with the search results.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Retrieves the current user ID from the token service</description></item>
        /// <item><description>Builds a query to fetch active sessions for the user</description></item>
        /// <item><description>Applies name filtering if a filter is provided</description></item>
        /// <item><description>Counts the total number of matching sessions</description></item>
        /// <item><description>Fetches the paginated list of sessions ordered by creation date descending</description></item>
        /// <item><description>Constructs and returns a paginated response DTO</description></item>
        /// </list>
        /// </remarks>
        Task<PaginatedResponseDto<SessionDto>> SearchSessionsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the token count for the system prompt using the specified model's tokenizer.
        /// </summary>
        /// <param name="modelName">The name of the model to use for tokenization.</param>
        /// <returns>The number of tokens in the default system prompt.</returns>
        /// <remarks>
        /// This method uses the Tiktoken tokenizer to count tokens based on the model's encoding scheme.
        /// The token count is useful for calculating context window usage and API costs.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown when the model name is null or empty.</exception>
        int GetSystemPromptTokenCount(string modelName);

        /// <summary>
        /// Deactivates a session asynchronously by setting its deactivation date and updating related entities.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session to deactivate.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Checks if the session exists and belongs to the current user and is not already deactivated</description></item>
        /// <item><description>If the session does not exist or is already deactivated, logs a warning and returns early</description></item>
        /// <item><description>Deactivates all document pages associated with documents in the session</description></item>
        /// <item><description>Deactivates all documents associated with the session</description></item>
        /// <item><description>Deactivates the session itself and updates its modification date</description></item>
        /// </list>
        /// </remarks>
        Task DeactivateSessionAsync(Guid sessionId, CancellationToken cancellationToken);

        /// <summary>
        /// Deactivates multiple sessions in bulk asynchronously based on the provided request.
        /// </summary>
        /// <param name="request">The request containing the list of session IDs to deactivate.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Validates the request and retrieves the current user ID</description></item>
        /// <item><description>Filters and retrieves valid session IDs that belong to the user and are not already deactivated</description></item>
        /// <item><description>If no valid sessions are found, logs a warning and returns early</description></item>
        /// <item><description>Deactivates all document pages associated with the sessions</description></item>
        /// <item><description>Deactivates all documents associated with the sessions</description></item>
        /// <item><description>Deactivates the sessions themselves and updates their modification dates</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request validation fails.</exception>
        Task DeactivateSessionBulkAsync(DeactivateSessionBulkActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing chat session asynchronously with the provided information.
        /// </summary>
        /// <param name="request">The request containing the session ID, updated name, and optional project ID.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="SessionDto"/> 
        /// with the updated session details.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Validates the request using the update session validator</description></item>
        /// <item><description>Retrieves the current user ID from the token service</description></item>
        /// <item><description>Updates the session name, project association, and modification date for sessions matching the ID and user</description></item>
        /// <item><description>Logs success if the session was updated, or a warning if no session was found</description></item>
        /// <item><description>Retrieves and returns the updated session details</description></item>
        /// </list>
        /// Only active sessions (not deactivated) belonging to the current user can be updated.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request validation fails.</exception>
        Task<SessionDto> UpdateSessionAsync(UpdateSessionActionDto request, CancellationToken cancellationToken);
    }

    public class SessionService(ILogger<SessionService> logger,
        [FromKeyedServices("azureaifoundry")] IChatClient openAiClient,
        ITokenService tokenService,
        IValidator<CreateSessionActionDto> createSessionValidator,
        IValidator<UpdateSessionActionDto> updateSessionValidator,
        IValidator<CreateChatStreamActionDto> createChatStreamValidator,
        IValidator<DeactivateSessionBulkActionDto> deactivateSessionBulkValidator,
        AIChatDbContext ctx) : ISessionService
    {
        private readonly ILogger<SessionService> _logger = logger;
        private readonly IChatClient _chatClient = openAiClient;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IValidator<CreateSessionActionDto> _createSessionValidator = createSessionValidator;
        private readonly IValidator<UpdateSessionActionDto> _updateSessionValidator = updateSessionValidator;
        private readonly IValidator<CreateChatStreamActionDto> _createChatStreamValidator = createChatStreamValidator;
        private readonly IValidator<DeactivateSessionBulkActionDto> _deactivateSessionBulkValidator = deactivateSessionBulkValidator;
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

        /// <inheritdoc />
        public async Task<SessionDto> GetSessionsAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid()!.Value;

            var session = await _ctx.Sessions
                .AsNoTracking()
                .Where(s => s.Id == sessionId && s.UserId == userId && !s.DateDeactivated.HasValue)
                .Select(s => s.MapToSessionDto())
                .FirstOrDefaultAsync(cancellationToken);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found");
            }

            return session;
        }

        /// <inheritdoc />
        public async Task<SessionDto> CreateChatSessionAsync(CreateSessionActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _createSessionValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

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
            var newSession = new Session()
            {
                Name = "New Chat",
                UserId = userId,
                DateCreated = date,
                DateModified = date
            };
            await _ctx.AddAsync(newSession, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            var prompt = string.Format(_defaultSystemPrompt, newSession.Id, userId);
            var coversations = new List<ChatMessage>
            {
                new(ChatRole.System, prompt)
            };
            newSession.Chat = new Chat()
            {
                Id = newSession.Id,
                UserId = userId,
                ProjectId = request.ProjectId,
                Name = newSession.Name,
                TotalTokens = 0,
                DateCreated = date,
                DateModified = date,
                Documents = [],
                Conversations = [new() 
                {
                    Id = Guid.NewGuid(),
                    Role = ChatRoles.System,
                    Content = prompt,
                    DateCreated = date
                }]
            };
            newSession.DateModified = date;
            await _ctx.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return newSession.MapToSessionDto();
        }

        /// <inheritdoc />
        public async Task<string> CreateSessionNameAsync(Guid sessionId, CreateChatStreamActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _createChatStreamValidator.ValidateAndThrow(request);

            var session = await _ctx.Sessions.FindAsync([sessionId], cancellationToken);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found");
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

            var response = await _chatClient.GetResponseAsync([
                                 new ChatMessage(ChatRole.System, _defaultSystemPrompt),
                                 new ChatMessage(ChatRole.User, $"Create a session name based on the following prompt, please make it 25 maximum and make it a string. Do not have the name on the session nor the id. Just the name based on the prompt. The result must be a string, not markdown. Prompt: {request.Prompt}")
                             ], new() { ModelId = modelName }, cancellationToken);
            if (response == null)
            {
                _logger.LogError("Failed to create session name for session id {id}", sessionId);
                throw new InvalidOperationException($"Failed to create session name for id {sessionId}");
            }

            var name = response.Messages.Last().Text?.Trim() ?? string.Empty;
            session.Name = name;
            session.DateModified = DateTimeOffset.UtcNow;
            session.Chat!.Name= name;
            await _ctx.SaveChangesAsync(cancellationToken);

            return session.Name;
        }

        /// <inheritdoc />
        public async Task<PaginatedResponseDto<SessionDto>> SearchSessionsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var userId = _tokenService.GetOid()!.Value;

            var query = _ctx.Sessions
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.DateDeactivated.HasValue);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.Name) && EF.Functions.Like(x.Name, $"%{filter}%"));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.DateCreated)
                .Skip(skip)
                .Take(take)
                .Select(s => s.MapToSessionDto())
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<SessionDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = take,
                CurrentPage = (skip / take) + 1
            };
        }

        /// <inheritdoc />
        public int GetSystemPromptTokenCount(string modelName)
        {
            ArgumentException.ThrowIfNullOrEmpty(modelName);

            var tokenizer = TiktokenTokenizer.CreateForModel(modelName);
            return tokenizer.CountTokens(_defaultSystemPrompt);
        }

        /// <inheritdoc />
        public async Task DeactivateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid()!.Value;
            var date = DateTimeOffset.UtcNow;

            var sessionExists = await _ctx.Sessions
                .Where(x => x.Id == sessionId && x.UserId == userId && !x.DateDeactivated.HasValue)
                .AnyAsync(cancellationToken);

            if (!sessionExists)
            {
                _logger.LogWarning("Session with id {id} not found or already deactivated", sessionId);
                return;
            }

            await _ctx.SessionDocumentPages
                .Where(p => p.SessionDocument.SessionId == sessionId && !p.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.SessionDocuments
                .Where(d => d.SessionId == sessionId && !d.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(d => d
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.Sessions
                .Where(s => s.Id == sessionId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.DateDeactivated, date)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeactivateSessionBulkAsync(DeactivateSessionBulkActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _deactivateSessionBulkValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;
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

        /// <inheritdoc />
        public async Task<SessionDto> UpdateSessionAsync(UpdateSessionActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _updateSessionValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

            var anySession = await _ctx.Sessions
                .Where(x => x.Id == request.Id && x.UserId == userId && !x.DateDeactivated.HasValue)
                .AnyAsync(cancellationToken);
            if (!anySession)
            {
                _logger.LogWarning("Session not found or already deactivated");
                throw new InvalidOperationException($"Session not found or already deactivated.");
            }

            var rows = await _ctx.Sessions
                .Where(x => x.Id == request.Id && x.UserId == userId && !x.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Name, request.Name)
                    .SetProperty(x => x.ProjectId, request.ProjectId)
                    .SetProperty(x => x.DateModified, DateTimeOffset.UtcNow),
                    cancellationToken);

            _logger.LogInformation("Session {Id} successfully updated.", request.Id);

            return await GetSessionsAsync(request.Id, cancellationToken);
        }
    }
}
