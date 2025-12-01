using System.Text.Json.Serialization;

namespace RR.AI_Chat.Dto
{
    public class ProjectDocumentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("projectId")]
        public Guid ProjectId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
