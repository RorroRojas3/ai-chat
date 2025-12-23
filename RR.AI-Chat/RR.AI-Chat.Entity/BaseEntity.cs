using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RR.AI_Chat.Entity
{
    public class BaseEntity
    {
        [Key]
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("dateCreated")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonPropertyName("dateDeactivated")]
        public DateTimeOffset? DateDeactivated { get; set; }

        [Timestamp]
        [JsonPropertyName("version")]
        public byte[] Version { get; set; } = null!;
    }

    public class BaseModifiedEntity : BaseEntity
    {
        [JsonPropertyName("dateModified")]
        public DateTimeOffset DateModified { get; set; }
    }
}
