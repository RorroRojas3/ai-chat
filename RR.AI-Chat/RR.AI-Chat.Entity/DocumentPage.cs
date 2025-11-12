using Microsoft.Data.SqlTypes;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(DocumentPage), Schema = "Core")]
    public class DocumentPage : BaseEntity
    {
        [ForeignKey(nameof(Document))]
        public Guid DocumentId { get; set; }

        public int Number { get; set; }

        public string Text { get; set; } = null!;

        [Column(TypeName = "vector(1536)")]
        public SqlVector<float> Embedding { get; set; }

        public Document Document { get; set; } = null!;
    }
}
