using System.ComponentModel.DataAnnotations;

namespace RR.AI_Chat.Dto.Actions.User
{
    public class CreateUserActionDto
    {
        [StringLength(256)]
        public string FirstName { get; set; } = null!;

        [StringLength(256)]
        public string LastName { get; set; } = null!;

        [StringLength(512)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public bool IsSuperAdministrator { get; set; } = false;
    }
}
