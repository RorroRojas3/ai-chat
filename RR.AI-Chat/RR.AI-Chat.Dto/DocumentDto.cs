namespace RR.AI_Chat.Dto
{
    public class DocumentDto
    {
        public string Id { get; set; } = null!;

        public string SessionId { get; set; } = null!;

        public string DocumentId => Id;

        public string Name { get; set; } = null!;
    }
}
