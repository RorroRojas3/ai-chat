using System.Text.Json.Serialization;

namespace RR.AI_Chat.Dto
{
    public class SessionDocumentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } 

        [JsonPropertyName("sessionId")]
        public Guid SessionId { get; set; }

        [JsonPropertyName("documentId")]
        public Guid DocumentId => Id;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
