using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Session), Schema = "Core")]    
    public class Session : BaseModifiedEntity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = "New Chat";

        public List<Conversation>? Conversations { get; set; } = [];

        public long InputTokens { get; set; }

        public long OutputTokens { get; set; }

        public long TotalTokens => InputTokens + OutputTokens;

        public User User { get; set; } = null!;

        public List<Document> Documents { get; set; } = [];
    }
}
