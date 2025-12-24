using RR.AI_Chat.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RR.AI_Chat.Entity
{
    public class Chat
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("projectId")]
        public Guid? ProjectId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("totalTokens")]
        public long TotalTokens { get; set; }

        [JsonPropertyName("messageCount")]
        public long MessageCount => Conversations.Count;

        [JsonPropertyName("documents")]
        public List<ChatDocument> Documents { get; set; } = [];

        [JsonPropertyName("conversations")]
        public List<ChatConversation> Conversations { get; set; } = [];

        [JsonPropertyName("dateCreated")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonPropertyName("dateModified")]
        public DateTimeOffset DateModified { get; set; }

        [JsonPropertyName("dateDeactivated")]
        public DateTimeOffset? DateDeactivated { get; set; }
    }

    public class ChatDocument
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("extension")]
        public string Extension { get; set; } = null!;

        [JsonPropertyName("mimeType")]
        public string MimeType { get; set; } = null!;

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }

    public class ChatConversation
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("role")]
        public ChatRoles Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = null!;

        [JsonPropertyName("dateCreated")]
        public DateTimeOffset DateCreated { get; set; }

        [JsonPropertyName("tokens")]
        public long Tokens { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("usage")]
        public ChatUsage? Usage { get; set; }
    }

    public class ChatUsage 
    {
        [JsonPropertyName("inputTokens")]
        public long InputTokens { get; set; }

        [JsonPropertyName("outputTokens")]
        public long OutputTokens { get; set; }
    }
}
