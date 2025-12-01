using RR.AI_Chat.Dto;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ProjectDocument), Schema = "Core")]
    public class ProjectDocument : BaseDocument
    {
        public Guid ProjectId { get; set; }

        public Project Project { get; set; } = null!;

        public List<ProjectDocumentPage> Pages { get; set; } = [];
    }

    public static class ProjectDocumentExtensions
    {
        public static ProjectDocumentDto MapToProjectDocumentDto(this ProjectDocument document)
        {
            return new ProjectDocumentDto
            {
                Id = document.Id,
                Name = document.Name,
                ProjectId = document.ProjectId
            };
        }
    }
}
