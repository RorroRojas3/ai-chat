using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController(IDocumentService service) : ControllerBase
    {
        private readonly IDocumentService _service = service;

        [HttpPost("sessions/{sessionId}")]
        public async Task<IActionResult> CreateDocumentAsync([FromRoute] Guid sessionId, IFormFile file)
        {
            var document = await _service.CreateDocumentAsync(file, sessionId);
            return Created($"api/documents/{document.Id}", document);
        }
    }
}
