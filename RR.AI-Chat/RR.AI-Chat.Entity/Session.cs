using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Session), Schema = "AI")]    
    public class Session : BaseEntity
    {
        [StringLength(100)]
        public string? Name { get; set; }

        public List<Conversation>? Conversations { get; set; } = [];

        public int InputTokens { get; set; }

        public int OutputTokens { get; set; }

        public int TotalTokens => InputTokens + OutputTokens;

        public DateTime DateModified { get; set; } 
    }
}
