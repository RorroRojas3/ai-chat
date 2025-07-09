using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;
using System.Runtime.CompilerServices;
using System.Text;

namespace RR.AI_Chat.Service
{
    public interface IChatService 
    {
        Task<string> GetChatCompletionAsync(string systemPrompt, string prompt, CancellationToken cancellationToken);

        IAsyncEnumerable<string?> GetChatStreamingAsync(string prompt, CancellationToken cancellationToken);

        Task<SessionDto> CreateChatSessionAsync();

        IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, CancellationToken cancellationToken);

        Task<ChatCompletionDto> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto request, CancellationToken cancellationToken);

        Task<List<ModelDto>> GetModelsAsync();

        Task<List<SessionDto>> GetSessionsAsync();

        Task<SessionConversationDto> GetSessionConversationAsync(Guid sessionId);
    }

    public class ChatService(ILogger<ChatService> logger, 
        IChatClient chatClient, IConfiguration configuration, 
        IDocumentService documentService,
        IHttpContextAccessor httpContextAccessor,
        ChatStore chatStore, AIChatDbContext ctx) : IChatService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IChatClient _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        private readonly IConfiguration _configuration = configuration;
        private readonly IDocumentService _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        private readonly ChatStore _chatStore = chatStore;
        private readonly AIChatDbContext _ctx = ctx;
        private const string _documentAgentPrompt = "You are a Document Query Optimization Agent. For every user request about retrieving or analyzing information from document(s), automatically perform the following steps before executing:\n\n1. **Intent Extraction**\n   - Determine exactly what the user is asking for (e.g., summary, specific data points, definitions, statistics).\n\n2. **Ambiguity Resolution**\n   - Internally identify any vague or underspecified elements (document name, section, format, scope, time frame).\n   - If needed, internally generate the clarifying details without exposing them to the user.\n\n3. **Query Enhancement**\n   - Internally rewrite the request into a precise, unambiguous query that references document names, sections, page ranges, keywords, or data formats as appropriate.\n\n4. **Execution**\n   - Use the enhanced query to locate and extract exactly the information requested from the document(s).\n\n5. **Response Delivery**\n   - Present the final answer clearly and concisely, without displaying the internal refinement process or rewritten query.\n\nMaintain a user-friendly tone and ensure high accuracy by refining queries behind the scenes to eliminate misunderstandings.";

        /// <summary>
        /// Gets the chat completion asynchronously based on the provided question.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <param name="question">The question to send to the chat client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the chat response message.</returns>
        public async Task<string> GetChatCompletionAsync(string systemPrompt, string prompt, CancellationToken cancellationToken)
        {
            var chatOptions = new ChatOptions
            {
               // Assuming AIFunction is a subclass of AITool, you can cast the list to IList<AITool>
               Tools = _documentService.GetFunctions().Cast<AITool>().ToList(),
               AllowMultipleToolCalls = true,
            };

            var sessionId = _httpContextAccessor.HttpContext?.Request.Headers["sessionId"].FirstOrDefault();
            systemPrompt = $"""
                You are an AI assistant helping user RorroRojas3 analyze documents.
                Current session ID: {sessionId}
        
                IMPORTANT WORKFLOW RULES:
                1. When user asks for document overviews, ALWAYS call GetSessionDocumentsAsync FIRST
                2. Only after getting session documents, call GetDocumentOverviewAsync with valid document IDs obtained from GetSessionDocumentsAsync
                3. If user asks for "overview" without specifying document, choose the most recent or relevant document
                4. Never call GetDocumentOverviewAsync without first knowing what documents exist
        
                Available functions:
                - GetSessionDocumentsAsync: Gets all documents in session (call this first)
                - GetDocumentOverviewAsync: Creates overview for specific document (call after getting documents)
        
                Always follow this sequence for overview requests:
                Step 1: Call GetSessionDocumentsAsync
                Step 2: Analyze returned documents
                Step 3: Call GetDocumentOverviewAsync with appropriate document ID
                """;

            var response = await _chatClient.GetResponseAsync([
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, prompt),
            ], chatOptions, cancellationToken);

            return response.Messages.Last().Text ?? string.Empty;
        }

        /// <summary>
        /// Streams chat responses asynchronously based on the provided prompt.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <param name="prompt">The prompt to send to the chat client.</param>
        /// <returns>An asynchronous stream of chat response messages.</returns>
        public async IAsyncEnumerable<string?> GetChatStreamingAsync(string prompt, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var message in _chatClient.GetStreamingResponseAsync(prompt, cancellationToken: cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return message.Text;
            }
        }

        public async Task<SessionDto> CreateChatSessionAsync()
        {
            var newSession = new Session() { DateCreated = DateTime.UtcNow };    
            await _ctx.AddAsync(newSession);
            await _ctx.SaveChangesAsync();

            var chatSession = new ChatSesion
            {
                SessionId = newSession.Id,
                Messages =
                [
                    new ChatMessage(ChatRole.System, "You are a helpful AI assistant.")
                ]
            };
            _chatStore.Sessions.Add(chatSession);

            return new() { Id = newSession.Id };
        }

        public async IAsyncEnumerable<string?> GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var session = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId) ?? throw new InvalidOperationException($"Session with id {sessionId} not found.");
            if (session.Messages.Count == 1)
            {
                var sessionName = await CreateSessionNameAsync(sessionId, request);
                session.Name = sessionName;
            }

            session.Messages.Add(new ChatMessage(ChatRole.User, request.Prompt));


            StringBuilder sb = new();
            await foreach (var message in _chatClient.GetStreamingResponseAsync(session.Messages ?? [], new ChatOptions() { ModelId = request.ModelId}, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                sb.Append(message.Text);
                yield return message.Text;
            }

            session.Messages?.Add(new ChatMessage(ChatRole.Assistant, sb.ToString()));
        }

        public async Task<ChatCompletionDto> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto prompt, CancellationToken cancellationToken)
        {
            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.User, prompt.Prompt));
            
            var messages = _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages;
            var response = await _chatClient.GetResponseAsync(messages ?? [], null,cancellationToken);

            _chatStore.Sessions.FirstOrDefault(s => s.SessionId == sessionId)?.Messages.Add(new ChatMessage(ChatRole.System, response.Messages[0].Text));
            
            return new() 
            { 
                SessionId = sessionId, 
                Message = response.Messages[0].Text, 
                InputTokenCount = response?.Usage?.InputTokenCount,
                OutputTokenCount = response?.Usage?.OutputTokenCount,
                TotalTokenCount = response?.Usage?.TotalTokenCount
            };
        }

        /// <summary>
        /// Retrieves a list of available models asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of ModelDto objects.</returns>
        public async Task<List<ModelDto>> GetModelsAsync()
        {
            var models = await _ctx.Models
                            .AsNoTracking()
                            .Select(x => new ModelDto
                            {
                                Id = x.Id,
                                Name = x.Name
                            })
                            .ToListAsync();

            return models;
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

            var response = await _chatClient.GetResponseAsync([
                                 new ChatMessage(ChatRole.System, "You are a helpful AI assistant."),
                                 new ChatMessage(ChatRole.User, $"Create a session name based on the following prompt, please make it 50 characters or less and make it a string, not markdown. Prompt: {request.Prompt}")
                             ], new() { ModelId = request.ModelId }, CancellationToken.None);
            if (response == null)
            {
                _logger.LogError("Failed to create session name for session id {id}", sessionId);
                throw new InvalidOperationException($"Failed to create session name for id {sessionId}");
            }

            return response.Messages[0].Text ?? string.Empty;
        }
    }
}
