﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace RR.AI_Chat.Service
{
    public interface IBlobStorageService
    {
        Task<byte[]> DownloadAsync(string container, string blob, CancellationToken cancellationToken);

        Task UploadAsync(string container, string blob, byte[] data, Dictionary<string, string> metadata, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(string container, string blob, CancellationToken cancellationToken);

        Uri GenerateSasUri(string container, string blob, TimeSpan expiresIn, BlobSasPermissions permissions);
    }

    public class BlobStorageService(ILogger<BlobStorageService> logger, 
        BlobServiceClient blobServiceClient) : IBlobStorageService
    {
        private readonly ILogger<BlobStorageService> _logger = logger;
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

        public async Task<byte[]> DownloadAsync(string container, string blob, CancellationToken cancellationToken)
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

        public Uri GenerateSasUri(string container, string blob, TimeSpan expiresIn, BlobSasPermissions permissions)
        {
            _logger.LogInformation("Generating SAS URI for blob '{Blob}' in container '{Container}' with permissions '{Permissions}' expiring in {ExpiresIn}",
                blob, container, permissions, expiresIn);

            var blobClient = _blobServiceClient.GetBlobContainerClient(container).GetBlobClient(blob);

            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogError("BlobClient cannot generate SAS URI. Ensure the BlobServiceClient is configured with credentials that support SAS generation.");
                throw new InvalidOperationException("BlobClient cannot generate SAS URI. Ensure the BlobServiceClient is configured with credentials that support SAS generation.");
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = container,
                BlobName = blob,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn)
            };

            sasBuilder.SetPermissions(permissions);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            _logger.LogInformation("SAS URI generated successfully for blob '{Blob}' in container '{Container}'", blob, container);

            return sasUri;
        }
    }
}
