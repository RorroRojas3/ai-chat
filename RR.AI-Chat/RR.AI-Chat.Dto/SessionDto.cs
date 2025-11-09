namespace RR.AI_Chat.Dto
{
    public class SessionDto
    {
        public Guid Id { get; set; }

        public Guid? ProjectId { get; set; }

        public string Name { get; set; } = null!;

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }
    }

    public class SessionConversationDto : SessionDto
    {
        public List<SessionMessageDto> Messages { get; set; } = [];
    }
}
