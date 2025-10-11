using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Enums;
using RR.AI_Chat.Service;

namespace RR.AI_Chat.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController(IDocumentService service, IStorageConnection storageConnection) : ControllerBase
    {
        private readonly IDocumentService _service = service;
        private readonly IStorageConnection _storageConnection = storageConnection;

        [HttpPost("sessions/{sessionId}")]
        public async Task<IActionResult> CreateDocumentAsync([FromRoute] Guid sessionId, IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required and cannot be empty.");
            }

            // Extract file data before enqueueing the job
            var fileData = new FileDataDto
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length,
                Content = await ReadFileAsync(file)
            };
            var jobId = BackgroundJob.Enqueue(() => _service.CreateDocumentAsync(null, fileData, sessionId, cancellationToken));

            return Accepted(new JobDto { Id = jobId});
        }

        [HttpGet("upload-status/{jobId}")]
        public IActionResult GetJobStatus(string jobId)
        {
            var jobData = _storageConnection.GetJobData(jobId);
            if (jobData == null)
                return NotFound();

            var statusParam = _storageConnection.GetJobParameter(jobId, JobName.Status.ToString());
            var progressParam = _storageConnection.GetJobParameter(jobId, JobName.Progress.ToString());

            // Deserialize the JSON-encoded values
            var status = SerializationHelper.Deserialize<string>(statusParam) ?? JobStatus.Queued.ToString();
            int progress = SerializationHelper.Deserialize<int>(progressParam);
 
            return Ok(new JobStatusDto
            {
                Id = jobId,
                State = jobData.State,
                Status = status,
                Progress = progress
            });
        }

        private static async Task<byte[]> ReadFileAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
