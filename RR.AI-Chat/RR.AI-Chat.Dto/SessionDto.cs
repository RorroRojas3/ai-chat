namespace RR.AI_Chat.Dto
{
    public class SessionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }
    }

    public class SessionConversationDto : SessionDto
    {
        public List<SessionMessageDto> Messages { get; set; } = [];
    }
}
