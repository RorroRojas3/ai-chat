using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController(ISessionService sessionService) : ControllerBase
    {
        private readonly ISessionService _sessionService = sessionService;  

        [HttpPost]
        public async Task<IActionResult> CreateSessionAsync(CancellationToken cancellationToken)
        {
            var response = await _sessionService.CreateChatSessionAsync(cancellationToken);
            return Created($"api/sessions/{response.Id}", response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchSessionsAsync([FromQuery] string? filter, [FromQuery] int skip, [FromQuery] int take, CancellationToken cancellationToken)
        {
            var response = await _sessionService.SearchSessionsAsync(filter, skip, take, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeactivateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            await _sessionService.DeactivateSessionAsync(sessionId, cancellationToken);
            return NoContent();
        }
    }
}
