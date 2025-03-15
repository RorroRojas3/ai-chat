using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Session), Schema = "AI")]    
    public class Session : BaseEntity
    {
    }
}
