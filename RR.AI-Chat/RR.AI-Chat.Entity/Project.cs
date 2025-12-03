using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Project), Schema = "Core")]
    public class Project : BaseModifiedEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = null!;

        [StringLength(1024)]
        public string? Description { get; set; }

        [StringLength(2048)]
        public string? Instructions { get; set; }

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

        public static ProjectDetailDto MapToProjectDetailDto(this Project source)
        {
            return new ProjectDetailDto
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
                Instructions = source.Instructions,
                DateCreated = source.DateCreated,
                DateModified = source.DateModified,
                Sessions = [.. source.Sessions.Select(s => s.MapToSessionDto())]
            };
        }
    }
}
