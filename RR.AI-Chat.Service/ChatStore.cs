using Microsoft.Extensions.AI;

namespace RR.AI_Chat.Service
{
    public class ChatStore
    {
        public List<ChatSesion> Sessions { get; set; } = new()!;
    }

    public class ChatSesion
    {
        public Guid SessionId { get; set; }

        public List<ChatMessage> Messages { get; set; } = null!;
    }
}
