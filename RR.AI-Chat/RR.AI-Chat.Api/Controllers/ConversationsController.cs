using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto.Actions.Chat;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationsController(IConversationService chatService) : ControllerBase
    {
        private readonly IConversationService _chatService = chatService;

        [HttpPost("{id}/stream")]
        public async Task GetChatStreamingAsync(Guid id, CreateConversationStreamActionDto request, CancellationToken cancellationToken)
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            await foreach (var message in _chatService.StreamConversationAsync(id, request, cancellationToken))
            {
                await Response.WriteAsync($"{message}", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }

        [HttpGet("{id}/histories")]
        public async Task<IActionResult> GetConversationHistoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await _chatService.GetConversationHistoryAsync(id, cancellationToken);
            return Ok(response);
        }
    }
}
