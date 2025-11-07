using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    public class SessionProject : BaseModifiedEntity
    {
        [ForeignKey(nameof(Session))]
        public Guid Guid { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = null!;

        [StringLength(2048)]
        public string Instructions { get; set; } = null!;   

        public Session Session { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
