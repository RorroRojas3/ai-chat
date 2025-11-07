using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface ISessionService
    {
        /// <summary>
        /// Creates a new chat session asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="SessionDto"/> 
        /// with the unique identifier of the newly created session.
        /// </returns>
        /// <remarks>
        /// This method performs the following operations:
        /// <list type="number">
        /// <item><description>Creates a new <see cref="Session"/> entity with the current UTC timestamp</description></item>
        /// <item><description>Persists the session to the database</description></item>
        /// <item><description>Creates an in-memory <see cref="ChatSession"/> with the default system prompt</description></item>
        /// <item><description>Adds the chat session to the chat store for runtime access</description></item>
        /// </list>
        /// The newly created session is initialized with a system message containing the default assistant prompt.
        /// </remarks>
        /// <exception cref="DbUpdateException">Thrown when the database update operation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity framework context is in an invalid state.</exception>
        Task<SessionDto> CreateChatSessionAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates a session name asynchronously based on the provided request.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="request">The request containing the prompt and model ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated session name.</returns>
        /// <exception cref="ArgumentException">Thrown when the request is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the session name creation fails.</exception>
        Task<string> CreateSessionNameAsync(Guid sessionId, ChatStreamRequestdto request, CancellationToken cancellationToken);

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
        Task DeactivateSessionBulkAsync(DeactivateSessionBulkActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Renames a session asynchronously based on the provided request.
        /// </summary>
        /// <param name="request">The request containing the session ID and the new name.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request validation fails.</exception>
        Task RenameSessionAsync(RenameSessionActionDto request, CancellationToken cancellationToken);
    }

    public class SessionService(ILogger<SessionService> logger,
        [FromKeyedServices("azureaifoundry")] IChatClient openAiClient,
        ITokenService tokenService,
        IValidator<RenameSessionActionDto> renameSessionValidator,
        AIChatDbContext ctx) : ISessionService
    {
        private readonly ILogger<SessionService> _logger = logger;
        private readonly IChatClient _chatClient = openAiClient;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IValidator<RenameSessionActionDto> _renameSessionValidator = renameSessionValidator;
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
        public async Task<SessionDto> CreateChatSessionAsync(CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid()!.Value;

            var transaction = await _ctx.Database.BeginTransactionAsync(cancellationToken);
            var date = DateTimeOffset.UtcNow;
            var newSession = new Session()
            {
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
            newSession.Conversations = [.. coversations.Select(x => new Conversation(x.Role, x.Text))];
            newSession.DateModified = date;
            await _ctx.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new() { Id = newSession.Id };
        }

        /// <inheritdoc />
        public async Task<string> CreateSessionNameAsync(Guid sessionId, ChatStreamRequestdto request, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(request));

            var session = await _ctx.Sessions.FindAsync([sessionId], cancellationToken);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found");
            }

            var model = await _ctx.Models
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == request.ModelId, cancellationToken);
            if (model == null)
            {
                _logger.LogError("Model with id {id} not found", request.ModelId);
                throw new InvalidOperationException($"Model with id {request.ModelId} not found");
            }

            var response = await _chatClient.GetResponseAsync([
                                 new ChatMessage(ChatRole.System, _defaultSystemPrompt),
                                 new ChatMessage(ChatRole.User, $"Create a session name based on the following prompt, please make it 25 maximum and make it a string. Do not hav the name on the session nor the id. Just the name based on the prompt. The result must be a string, not markdown. Prompt: {request.Prompt}")
                             ], new() { ModelId = model.Name }, cancellationToken);
            if (response == null)
            {
                _logger.LogError("Failed to create session name for session id {id}", sessionId);
                throw new InvalidOperationException($"Failed to create session name for id {sessionId}");
            }

            var name = response.Messages.Last().Text?.Trim() ?? string.Empty;
            session.Name = name;
            session.DateModified = DateTimeOffset.UtcNow;
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
                .Select(s => new SessionDto
                {
                    Id = s.Id,
                    Name = s.Name!,
                    DateCreated = s.DateCreated,
                    DateModified = s.DateModified
                })
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

            await _ctx.DocumentPages
                .Where(p => p.Document.SessionId == sessionId && !p.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.Documents
                .Where(d => d.SessionId == sessionId && !d.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(d => d
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.SessionProjects
                .Where(sp => sp.SessionId == sessionId && !sp.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(sp => sp
                    .SetProperty(x => x.DateModified, date),
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
            await _ctx.DocumentPages
                .Where(p => sessionIds.Contains(p.Document.SessionId) && !p.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.DateDeactivated, date),
                    cancellationToken);

            await _ctx.Documents
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
        public async Task RenameSessionAsync(RenameSessionActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _renameSessionValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

            var rows = await _ctx.Sessions
                .Where(x => x.Id == request.Id && x.UserId == userId && !x.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Name, request.Name)
                    .SetProperty(x => x.DateModified, DateTimeOffset.UtcNow),
                    cancellationToken);

            if (rows > 0)
            {
                _logger.LogInformation("Session {Id} successfully renamed session.", request.Id);
            }
            else
            {
                _logger.LogWarning("Session {Id} not found or could not be renamed.", request.Id);
            }
        }
    }
}
