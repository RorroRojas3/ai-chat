using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace RR.AI_Chat.Service
{
    public interface IBlobStorageService
    {
        Task<byte[]> DownlodAsync(string container, string blob, CancellationToken cancellationToken);

        Task UploadAsync(string container, string blob, byte[] data, Dictionary<string, string> metadata, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(string container, string blob, CancellationToken cancellationToken);
    }

    public class BlobStorageService(ILogger<BlobStorageService> logger, 
        BlobServiceClient blobServiceClient) : IBlobStorageService
    {
        private readonly ILogger<BlobStorageService> _logger = logger;
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

        public async Task<byte[]> DownlodAsync(string container, string blob, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Downloading blob '{Blob}' from container '{Container}'", blob, container); 

            var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blob);
            var downloadInfo = await blobClient.DownloadAsync(cancellationToken);

            _logger.LogInformation("Blob '{Blob}' downloaded successfully from container '{Container}'", blob, container);

            using var memoryStream = new MemoryStream();
            await downloadInfo.Value.Content.CopyToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }

        public async Task UploadAsync(string container, string blob, byte[] data, Dictionary<string, string> metadata, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Uploading blob '{Blob}' to container '{Container}'", blob, container);

            var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blob);
            using var memoryStream = new MemoryStream(data);

            var options = new BlobUploadOptions
            {
                Metadata = metadata
            };
            await blobClient.UploadAsync(memoryStream, options, cancellationToken);

            _logger.LogInformation("Blob '{Blob}' uploaded successfully to container '{Container}'", blob, container);
        }

        public async Task<bool> DeleteAsync(string container, string blob, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting blob '{Blob}' from container '{Container}'", blob, container);

            var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blob);
            var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            if (response.Value)
            {
                _logger.LogInformation("Blob '{Blob}' deleted successfully from container '{Container}'", blob, container);
            }
            else
            {
                _logger.LogWarning("Blob '{Blob}' not found in container '{Container}'", blob, container);
            }

            return response.Value;
        }
    }
}
