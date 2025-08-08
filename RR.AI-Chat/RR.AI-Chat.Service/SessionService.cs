using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface ISessionService
    {
        Task<SessionDto> CreateChatSessionAsync();

        Task<string> CreateSessionNameAsync(Guid sessionId, ChatStreamRequestdto request);

        Task<List<SessionDto>> SearchSessionsAsync(string? query);
    }

    public class SessionService(ILogger<SessionService> logger,
        [FromKeyedServices("azureaifoundry")] IChatClient openAiClient,
        AIChatDbContext ctx) : ISessionService
    {
        private readonly ILogger<SessionService> _logger = logger;
        private readonly IChatClient _chatClient = openAiClient;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly string _defaultSystemPrompt = @"
            You are an advanced AI assistant with comprehensive analytical capabilities and access to a powerful suite of specialized tools. Your primary mission is to provide thorough, insightful, and actionable responses that leverage all available resources to deliver maximum value.

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
            - **Structured Presentation**: Organize information logically with clear headings, sections, and formatting
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

            ## RESPONSE PHILOSOPHY:
            Excellence means leveraging every available capability to provide the most comprehensive, insightful, and valuable response possible. Don't just answer questions—anticipate needs, provide context, deliver transformative insights, and create responses that exceed expectations.

            Your session identifier is {0}. Use this for maintaining context and accessing session-specific resources throughout our conversation.

            Operate with invisible mastery: your sophisticated use of these capabilities should enhance every response without ever needing to explicitly mention the tools themselves.
            ";


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
        /// <item><description>Creates an in-memory <see cref="ChatSesion"/> with the default system prompt</description></item>
        /// <item><description>Adds the chat session to the chat store for runtime access</description></item>
        /// </list>
        /// The newly created session is initialized with a system message containing the default assistant prompt.
        /// </remarks>
        /// <exception cref="DbUpdateException">Thrown when the database update operation fails.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the entity framework context is in an invalid state.</exception>
        public async Task<SessionDto> CreateChatSessionAsync()
        {
            var transaction = await _ctx.Database.BeginTransactionAsync();
            var date = DateTime.UtcNow;
            var newSession = new Session() 
            { 
                DateCreated = date,
                DateModified = date
            };
            await _ctx.AddAsync(newSession);
            await _ctx.SaveChangesAsync();

            var prompt = string.Format(_defaultSystemPrompt, newSession.Id);
            var coversations = new List<ChatMessage>
            {
                new(ChatRole.System, prompt)
            };
            newSession.Conversations = [.. coversations.Select(x => new Conversation(x.Role, x.Text))];
            newSession.DateModified = date;
            await _ctx.SaveChangesAsync();
            await transaction.CommitAsync();

            return new() { Id = newSession.Id };
        }

        /// <summary>
        /// Creates a session name asynchronously based on the provided request.
        /// </summary>
        /// <param name="sessionId">The unique identifier of the session.</param>
        /// <param name="request">The request containing the prompt and model ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated session name.</returns>
        /// <exception cref="ArgumentException">Thrown when the request is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the session name creation fails.</exception>
        public async Task<string> CreateSessionNameAsync(Guid sessionId, ChatStreamRequestdto request)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(request));
            ArgumentException.ThrowIfNullOrWhiteSpace(nameof(request));

            var session = await _ctx.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                _logger.LogError("Session with id {id} not found", sessionId);
                throw new InvalidOperationException($"Session with id {sessionId} not found");
            }

            var model = await _ctx.Models.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ModelId);
            if (model == null)
            {
                _logger.LogError("Model with id {id} not found", request.ModelId);
                throw new InvalidOperationException($"Model with id {request.ModelId} not found");
            }

            var response = await _chatClient.GetResponseAsync([
                                 new ChatMessage(ChatRole.System, _defaultSystemPrompt),
                                 new ChatMessage(ChatRole.User, $"Create a session name based on the following prompt, please make it 25 maximum and make it a string. Do not hav the name on the session nor the id. Just the name based on the prompt. The result must be a string, not markdown. Prompt: {request.Prompt}")
                             ], new() { ModelId = model.Name }, CancellationToken.None);
            if (response == null)
            {
                _logger.LogError("Failed to create session name for session id {id}", sessionId);
                throw new InvalidOperationException($"Failed to create session name for id {sessionId}");
            }

            var name = response.Messages.Last().Text?.Trim() ?? string.Empty;
            session.Name = name;
            await _ctx.SaveChangesAsync();

            return session.Name;
        }

        /// <summary>
        /// Asynchronously searches for chat sessions based on a query string or returns recent sessions if no query is provided.
        /// </summary>
        /// <param name="query">The search query to filter sessions by name. If null or whitespace, returns recent sessions instead.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of up to 10 <see cref="SessionDto"/> 
        /// objects matching the search criteria, ordered by creation date (most recent first) when no query is provided.
        /// </returns>
        /// <remarks>
        /// This method performs different operations based on the query parameter:
        /// <list type="bullet">
        /// <item><description>If <paramref name="query"/> is null or whitespace: Returns the 10 most recent sessions with non-empty names, ordered by creation date descending</description></item>
        /// <item><description>If <paramref name="query"/> has a value: Returns up to 10 sessions whose names contain the query string (case-insensitive partial match)</description></item>
        /// </list>
        /// The method uses Entity Framework's <see cref="EF.Functions.Like"/> for SQL LIKE pattern matching and 
        /// <see cref="EntityFrameworkQueryableExtensions.AsNoTracking"/> for read-only operations to improve performance.
        /// Only sessions with non-null and non-whitespace names are included in the results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the database context is in an invalid state.</exception>
        /// <exception cref="SqlException">Thrown when a database-related error occurs during query execution.</exception>
        public async Task<List<SessionDto>> SearchSessionsAsync(string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _ctx.Sessions.AsNoTracking()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                    .OrderByDescending(x => x.DateCreated)
                    .Take(10)
                    .Select(s => new SessionDto { Id = s.Id, Name = s.Name! })
                    .ToListAsync();
            }

            var sessions = await _ctx.Sessions.AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.Name) &&
                            EF.Functions.Like(x.Name, $"%{query}%"))
                .Take(10)
                .Select(s => new SessionDto { Id = s.Id, Name = s.Name! })
                .ToListAsync();
            return sessions;
        }
    }
}
