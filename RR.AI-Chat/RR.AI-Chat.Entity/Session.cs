using System.ComponentModel.DataAnnotations.Schema;

namespace RR.AI_Chat.Entity
{
    [Table(nameof(Session), Schema = "AI")]    
    public class Session : BaseEntity
    {
        public Session(string name)
        {
            Name = name;
            DateCreated = DateTime.UtcNow;
        }

        public string Name { get; set; } = "New Chat";
    }
}
