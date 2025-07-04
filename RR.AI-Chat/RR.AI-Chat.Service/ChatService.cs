﻿using Microsoft.EntityFrameworkCore;
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
        Task<string> GetChatCompletionAsync(string question, CancellationToken cancellationToken);

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
        ChatStore chatStore, AIChatDbContext ctx) : IChatService
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IChatClient _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        private readonly IConfiguration _configuration = configuration;
        private readonly ChatStore _chatStore = chatStore;
        private readonly AIChatDbContext _ctx = ctx;

        /// <summary>
        /// Gets the chat completion asynchronously based on the provided question.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <param name="question">The question to send to the chat client.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the chat response message.</returns>
        public async Task<string> GetChatCompletionAsync(string question, CancellationToken cancellationToken)
        {
            var response = await _chatClient.GetResponseAsync([
                new ChatMessage(ChatRole.System, "You are a helpful AI assistant"),
                new ChatMessage(ChatRole.User, question),
            ], null, cancellationToken);

            return response.Messages[0].Text ?? string.Empty;
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
            var newSession = new Session();    
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
