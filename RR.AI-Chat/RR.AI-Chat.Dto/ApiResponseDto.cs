using System.Net;
using System.Text.Json.Serialization;

namespace RR.AI_Chat.Dto
{
    public class ApiResponseDto
    {
        [JsonPropertyName("statusCode")]
        public HttpStatusCode StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = null!;
    }
}
