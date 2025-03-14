using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Document), Schema = "AI")]
    public class Document : BaseEntity
    {
        [ForeignKey(nameof(Session))]
        public Guid SessionId { get; set; }

        public string Name { get; set; } = null!;

        public string Extension { get; set; } = null!;

        public Session Session { get; set; } = null!;

        public List<DocumentPage> Pages { get; set; } = [];  
    }
}
