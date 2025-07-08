using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Repository;
using System.Text.Json;

namespace RR.AI_Chat.Service
{
    public interface IDocumentFunctionService
    {
        Task<ChatOptions> GetDocumentFunctionsAsync();
    }

    public class DocumentFunctionService(ILogger<DocumentFunctionService> logger, 
        IHttpContextAccessor httpContextAccessor,
        AIChatDbContext ctx) : IDocumentFunctionService
    {
        private readonly ILogger _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<ChatOptions> GetDocumentFunctionsAsync()
        {
            await Task.CompletedTask;
            return new()
            {
                Tools = [GetSessionDocumentsAsync()]
            };
        }

        public AIFunction GetSessionDocumentsAsync()
        {
            return AIFunctionFactory.Create(() =>
            {
                var headers = _httpContextAccessor.HttpContext?.Request.Headers;

                var sessionId = headers?["sessionId"].FirstOrDefault();
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new ArgumentException("Session ID is required.");
                }

                var documents = _ctx.Documents.AsNoTracking()
                    .Where(x => x.SessionId == Guid.Parse(sessionId))
                    .Select(x => new DocumentDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                    }).ToList();

                return JsonSerializer.Serialize(documents);
            }, "get_session_documents", "Get the documents for the current session.");   
        }
    }
}