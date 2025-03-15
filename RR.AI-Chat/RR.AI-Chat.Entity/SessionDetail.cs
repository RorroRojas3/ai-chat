using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(SessionDetail), Schema = "AI")]
    public class SessionDetail : BaseEntity
    {
        [ForeignKey(nameof(Session))]
        public Guid SessionId { get; set; }

        public Guid ModelId { get; set; }

        [StringLength(25)]
        public string Name { get; set; } = null!;

        public Session Session { get; set; } = null!;

        public Model Model { get; set; } = null!;
    }
}
