using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController(ISessionService sessionService) : ControllerBase
    {
        private readonly ISessionService _sessionService = sessionService;  

        [HttpPost]
        public async Task<IActionResult> CreateSessionAsync()
        {
            var response = await _sessionService.CreateChatSessionAsync();
            return Created($"api/sessions/{response.Id}", response);
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionsAsync()
        {
            var response = await _sessionService.GetSessionsAsync();
            return Ok(response);
        }
    }
}
