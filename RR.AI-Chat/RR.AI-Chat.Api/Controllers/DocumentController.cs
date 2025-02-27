using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController(IDocumentService service) : ControllerBase
    {
        private readonly IDocumentService _service = service;

        [HttpPost]
        public async Task<IActionResult> CreateDocumentAsync(IFormFile formFile)
        {
            var document = await _service.CreateDocumentAsync(formFile);
            return Created($"api/documents/{document.Id}", document);
        }
    }
}
