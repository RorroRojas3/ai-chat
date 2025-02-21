namespace RR.AI_Chat.Dto
{
    public class SessionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
    }

    public class SessionConversationDto : SessionDto
    {
        public List<SessionMessageDto> Messages { get; set; } = [];
    }
}
