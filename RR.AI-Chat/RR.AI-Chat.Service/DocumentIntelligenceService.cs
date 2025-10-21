using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RR.AI_Chat.Service
{
    public interface IDocumentIntelligenceService
    {
        Task<AnalyzeResult> ReadAsync(byte[] bytes, CancellationToken cancellationToken);
    }

    public class DocumentIntelligenceService(ILogger<DocumentIntelligenceService> logger,
        IConfiguration configuration,
        DocumentIntelligenceClient client) : IDocumentIntelligenceService
    {
        private readonly ILogger<DocumentIntelligenceService> _logger = logger;
        private readonly IConfiguration _configuration = configuration;
        private readonly DocumentIntelligenceClient _client = client;
        private readonly string _ocrModelId = configuration.GetValue<string>("DocumentIntelligence:OCRModelId")!;

        public async Task<AnalyzeResult> ReadAsync(byte[] bytes, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(bytes);

            var binaryData = BinaryData.FromBytes(bytes);
            var options = new AnalyzeDocumentOptions(_ocrModelId, binaryData);

            var result = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, options, cancellationToken);

            return result.Value;
        }
    }
}
