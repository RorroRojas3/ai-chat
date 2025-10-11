using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ModelsController(IModelService modelService) : ControllerBase
    {
        private readonly IModelService _modelService = modelService;

        [HttpGet]
        public async Task<IActionResult> GetModelsAsync()
        {
            var response = await _modelService.GetModelsAsync();
            return Ok(response);
        }
    }
}
