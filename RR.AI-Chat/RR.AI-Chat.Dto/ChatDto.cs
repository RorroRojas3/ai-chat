namespace RR.AI_Chat.Dto
{
    public class ChatDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }
    }

    public class ChatConversationDto : ChatDto
    {
        public List<ChatMessageDto> Messages { get; set; } = [];
    }
}
