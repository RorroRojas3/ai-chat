using Microsoft.Extensions.AI;

namespace RR.AI_Chat.Entity
{
    public class Conversation(ChatRole role, string content)
    {
        public ChatRole Role { get; set; } = role;

        public string Content { get; set; } = content;
    }
}
