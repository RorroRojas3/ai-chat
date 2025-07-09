namespace RR.AI_Chat.Dto
{
    public class DocumentDto
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public Guid DocumentId => Id;

        public string Name { get; set; } = null!;
    }
}
