using System.Text.Json.Serialization;

namespace RR.AI_Chat.Dto
{
    public class FileDto
    {
        public string FileName { get; set; } = null!;
        public byte[] Content { get; set; } = [];
        public string ContentType { get; set; } = null!;
        public long Length { get; set; }
    }

    public class FileBase64Dto
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = null!;

        [JsonPropertyName("base64")]
        public string Base64 { get; set; } = null!;

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = null!;

        [JsonPropertyName("length")]
        public long Length { get; set; }
    }
}