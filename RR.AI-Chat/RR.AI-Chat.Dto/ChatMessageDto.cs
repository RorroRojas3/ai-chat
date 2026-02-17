using RR.AI_Chat.Common.Enums;

namespace RR.AI_Chat.Dto
{
    public class ChatMessageDto
    {
        public string Text { get; set; } = null!;

        public ChatRoles Role { get; set; }
    }
}
