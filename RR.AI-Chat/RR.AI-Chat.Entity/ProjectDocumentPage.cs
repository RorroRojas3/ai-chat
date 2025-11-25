using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(ProjectDocumentPage), Schema = "Core")]
    public class ProjectDocumentPage : BaseDocumentPage
    {
        public Guid ProjectDocumentId { get; set; }

        public ProjectDocument ProjectDocument { get; set; } = null!;
    }
}
