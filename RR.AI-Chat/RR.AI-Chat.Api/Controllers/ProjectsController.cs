using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController(IProjectService projectService) : ControllerBase
    {
        private readonly IProjectService _projectService = projectService;

        [HttpGet("search")]
        public async Task<IActionResult> SearchProjectsAsync([FromQuery] string? filter, [FromQuery] int skip, [FromQuery] int take, CancellationToken cancellationToken)
        {
            var response = await _projectService.SearchProjectsAsync(filter, skip, take, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProjectAsync([FromBody] UpsertProjectActionDto request, CancellationToken cancellationToken)
        {
            var response = await _projectService.CreateProjectAsync(request, cancellationToken);
            return Created($"api/projects/{response.Id}", response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProjectAsync([FromBody] UpsertProjectActionDto request, CancellationToken cancellationToken)
        {
            var response = await _projectService.UpdateProjectAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}
