using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Session), Schema = "AI")]    
    public class Session : BaseModifiedEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [StringLength(100)]
        public string? Name { get; set; }

        public List<Conversation>? Conversations { get; set; } = [];

        public long InputTokens { get; set; }

        public long OutputTokens { get; set; }

        public long TotalTokens => InputTokens + OutputTokens;

        public DateTime DateModified { get; set; }

        public User User { get; set; } = null!;
    }
}
