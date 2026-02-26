namespace RR.AI_Chat.Dto
{
    public sealed class UserDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
