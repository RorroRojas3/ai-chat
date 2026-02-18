using System.Text.Json.Serialization;

namespace RR.AI_Chat.Dto
{
    public class ChatDocumentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } 

        [JsonPropertyName("chatId")]
        public Guid ChatId { get; set; }

        [JsonPropertyName("documentId")]
        public Guid DocumentId => Id;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }
}
