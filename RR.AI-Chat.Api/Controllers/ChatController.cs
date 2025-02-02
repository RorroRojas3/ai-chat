using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private IChatService _chatService;

        public ChatController(IChatService chatService) 
        {
            _chatService = chatService; 
        }

        [HttpPost("completion")]
        public async Task<IActionResult> GetChatCompletionAsync(CancellationToken cancellationToken, string question)
        {
            var response = await _chatService.GetChatCompletionAsync(cancellationToken, question);
            return Ok(response);   
        }

        [HttpPost("streaming")]
        public async Task GetChatStreamingAsync(CancellationToken cancellationToken, string prompt)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            await foreach (var message in _chatService.GetChatStreamingAsync(cancellationToken, prompt))
            {
                await Response.WriteAsync($"data: {message}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

    }
}
