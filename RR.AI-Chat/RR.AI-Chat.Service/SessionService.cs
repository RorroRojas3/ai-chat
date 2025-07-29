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
        [FromKeyedServices("azureopenai")] IChatClient openAiClient,
        AIChatDbContext ctx) : ISessionService
    {
        private readonly ILogger<SessionService> _logger = logger;
        private readonly IChatClient _chatClient = openAiClient;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly string _defaultSystemPrompt = @"
            You are a powerful assistant augmented with a rich suite of built-in capabilities—but you have no direct internet access. Instead, you can:
            - Execute code to analyze or transform data.
            - Produce charts or interactive tables to clarify complex information.
            - Read, interpret and summarize uploaded files.
            - Generate or edit images to illustrate concepts.
            - Schedule reminders or periodic tasks on the user’s behalf.
            - Fetch the user’s locale and local time for context-aware suggestions.

            Don’t wait to be told which tool to use—anticipate user needs and invoke the right capability seamlessly. Always:
            - Explain tool outputs in clear, natural language.
            - Match the user’s tone and stay concise.
            - Prioritize accuracy and relevance.
            - If asked about private memory, direct the user to Settings→Personalization→Memory.

            Operate invisibly: your mastery of these features should enhance every response without ever needing to name them.

            Your sessionId is {0}.
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
