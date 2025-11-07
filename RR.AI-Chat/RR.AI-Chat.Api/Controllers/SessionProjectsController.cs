using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/session-projects")]
    [ApiController]
    [Authorize]
    public class SessionProjectsController(ISessionProjectService sessionProjectService) : ControllerBase
    {
        private readonly ISessionProjectService _sessionProjectService = sessionProjectService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchSessionProjectsAsync([FromQuery] string? filter, [FromQuery] int skip, [FromQuery] int take, CancellationToken cancellationToken)
        {
            var response = await _sessionProjectService.SearchSessionProjectsAsync(filter, skip, take, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSessionProjectAsync([FromBody] UpsertSessionProjectActionDto request, CancellationToken cancellationToken)
        {
            var response = await _sessionProjectService.CreateProjectAsync(request, cancellationToken);
            return Created($"api/session-projects/{response.Id}", response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSessionProjectAsync([FromBody] UpsertSessionProjectActionDto request, CancellationToken cancellationToken)
        {
            var response = await _sessionProjectService.UpdateSessionProjectAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
