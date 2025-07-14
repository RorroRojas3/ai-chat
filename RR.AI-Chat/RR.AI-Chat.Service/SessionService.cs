using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface ISessionService
    {
        Task<SessionDto> CreateChatSessionAsync();
    }

    public class SessionService(ILogger<SessionService> logger,
        AIChatDbContext ctx,
        ChatStore chatStore) : ISessionService
    {
        private readonly ILogger<SessionService> _logger = logger;
        private readonly ChatStore _chatStore = chatStore;
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
            ";


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
                    new ChatMessage(ChatRole.System, _defaultSystemPrompt)
                ]
            };
            _chatStore.Sessions.Add(chatSession);

            return new() { Id = newSession.Id };
        }
    }
}
