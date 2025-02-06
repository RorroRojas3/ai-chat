namespace RR.AI_Chat.Dto
{
    public class ChatCompletionDto
    {
        public Guid SessionId { get; set; }

        public string? Message { get; set; }

        public int? InputTokenCount { get; set; }

        public int? OutputTokenCount { get; set; }

        public int? TotalTokenCount { get; set; }
    }
}
