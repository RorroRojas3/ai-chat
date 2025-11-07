using System.ComponentModel.DataAnnotations;

namespace RR.AI_Chat.Entity
{
    public class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset? DateDeactivated { get; set; }

        [Timestamp]
        public byte[] Version { get; set; } = null!;
    }

    public class BaseModifiedEntity : BaseEntity
    {
        public DateTimeOffset DateModified { get; set; }
    }
}
