using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController(IDocumentService service) : ControllerBase
    {
        private readonly IDocumentService _service = service;

        [HttpPost]
        public async Task<IActionResult> CreateDocumentAsync(IFormFile file)
        {
            var document = await _service.CreateDocumentAsync(file, Guid.Parse(Request.Headers["sessionId"].ToString()));
            return Created($"api/documents/{document.Id}", document);
        }

        [HttpPost("search")]
        public async Task<IActionResult> SearchDocumentsAsync(SearchDocumentRequestDto request)
        {
            var documents = await _service.SearchDocumentsAsync(request.SessionId, request);
            return Ok(documents);
        }
    }
}
