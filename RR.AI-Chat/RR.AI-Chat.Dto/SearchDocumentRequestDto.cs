namespace RR.AI_Chat.Dto
{
    public class SearchDocumentRequestDto
    {
        public Guid SessionId { get; set; }

        public string Prompt { get; set; } = null!;
    }
}
