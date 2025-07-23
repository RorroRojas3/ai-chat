using Microsoft.Extensions.AI;

namespace RR.AI_Chat.Entity
{
    public class Conversation
    {
        public Conversation(ChatRole role, string content)
        {
            Role = role;
            Content = content;
        }

        public ChatRole Role { get; set; }

        public string Content { get; set; } = null!;
    }
}
