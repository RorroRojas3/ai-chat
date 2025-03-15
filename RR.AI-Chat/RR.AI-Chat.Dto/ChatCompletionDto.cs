namespace RR.AI_Chat.Dto
{
    public class ChatCompletionDto
    {
        public Guid SessionId { get; set; }

        public string? Message { get; set; }

        public long? InputTokenCount { get; set; }

        public long? OutputTokenCount { get; set; }

        public long? TotalTokenCount { get; set; }
    }
}
