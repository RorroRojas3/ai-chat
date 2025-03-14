using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(IChatService chatService) : ControllerBase
    {
        private readonly IChatService _chatService = chatService;

        [HttpPost("completion")]
        public async Task<IActionResult> GetChatCompletionAsync(string question, CancellationToken cancellationToken)
        {
            var response = await _chatService.GetChatCompletionAsync(question, cancellationToken);
            return Ok(response);   
        }

        [HttpPost("streaming")]
        public async Task GetChatStreamingAsync(string prompt, CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            await foreach (var message in _chatService.GetChatStreamingAsync(prompt, cancellationToken))
            {
                await Response.WriteAsync($"data: {message}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpPost("session")]
        public async Task<IActionResult> CreateChatSessionAsync()
        {
            var dto = await _chatService.CreateChatSessionAsync();
            return Ok(dto);
        }

        [HttpPost("session/{sessionId}/stream")]
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

        [HttpPost("session/{sessionId}/completion")]
        public async Task<IActionResult> GetChatCompletionAsync(Guid sessionId, ChatCompletionRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _chatService.GetChatCompletionAsync(sessionId, request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModelsAsync()
        {
            var response = await _chatService.GetModelsAsync();
            return Ok(response);
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessionsAsync()
        {
            var response = await _chatService.GetSessionsAsync();
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
