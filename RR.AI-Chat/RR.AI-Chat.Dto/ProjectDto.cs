namespace RR.AI_Chat.Dto
{
    public class ProjectDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Instructions { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset DateModified { get; set; }
    }
}
