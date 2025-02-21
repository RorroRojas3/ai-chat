using RR.AI_Chat.Dto.Enums;

namespace RR.AI_Chat.Dto
{
    public class SessionMessageDto
    {
        public string Text { get; set; } = null!;

        public ChatRoleType Role { get; set; }
    }
}
