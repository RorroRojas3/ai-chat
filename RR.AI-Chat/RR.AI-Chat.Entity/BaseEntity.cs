using System.ComponentModel.DataAnnotations;

namespace RR.AI_Chat.Entity
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateDeactivated { get; set; }
    }
}
