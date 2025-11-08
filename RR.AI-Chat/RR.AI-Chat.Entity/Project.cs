using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    public class Project : BaseModifiedEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = null!;

        [StringLength(1024)]
        public string Description { get; set; } = null!;

        [StringLength(2048)]
        public string Instructions { get; set; } = null!;

        public User User { get; set; } = null!;

        public List<Session> Sessions { get; set; } = [];
    }

    public static class SessionProjectExtensions
    {
        public static ProjectDto MapToProjectDto(this Project source)
        {
            return new ProjectDto
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
