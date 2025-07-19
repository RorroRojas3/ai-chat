using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatsController(IChatService chatService) : ControllerBase
    {
        private readonly IChatService _chatService = chatService;

        [HttpPost("sessions/{sessionId}/stream")]
        public async Task GetChatStreamingAsync(Guid sessionId, ChatStreamRequestdto request, CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            await foreach (var message in _chatService.GetChatStreamingAsync(sessionId, request, cancellationToken))
            {
                await Response.WriteAsync($"{message}", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpPost("sessions/{sessionId}/completion")]
        public async Task<IActionResult> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _chatService.GetChatCompletionAsync(sessionId, request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("sessions/{sessionId}/conversations")]
        public async Task<IActionResult> GetSessionConversationAsync(Guid sessionId)
        {
            var response = await _chatService.GetSessionConversationAsync(sessionId);
            return Ok(response);
        }
    }
}
