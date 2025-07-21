namespace RR.AI_Chat.Dto
{
    public class ChatStreamRequestdto
    {
        public string Prompt { get; set; } = null!;

        public Guid ModelId { get; set; }

        public Guid ServiceId { get; set; } 
    }
}
