using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    public class SessionProject : BaseModifiedEntity
    {
        [ForeignKey(nameof(Session))]
        public Guid SessionId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = null!;

        [StringLength(1024)]
        public string Description { get; set; } = null!;

        [StringLength(2048)]
        public string Instructions { get; set; } = null!;

        public Session Session { get; set; } = null!;

        public User User { get; set; } = null!;
    }

    public static class SessionProjectExtensions
    {
        public static SessionProjectDto MapToSessionProjectDto(this SessionProject source)
        {
            return new SessionProjectDto
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                Instructions = source.Instructions,
                DateCreated = source.DateCreated,
                DateModified = source.DateModified
            };
        }
    }
}
