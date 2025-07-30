using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Memes.Services;

public class UploadCleanupService : BackgroundService
{
    private readonly ILogger<UploadCleanupService> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1); // Run daily
    private readonly TimeSpan _blobMaxAge = TimeSpan.FromHours(24); // Delete blobs older than 24 hours

    public UploadCleanupService(
        ILogger<UploadCleanupService> logger,
        BlobServiceClient blobServiceClient)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Upload cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldUploadsAsync(stoppingToken);
                
                // Wait for the next cleanup cycle
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during upload cleanup");
                
                // Wait a shorter time before retrying after an error
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        _logger.LogInformation("Upload cleanup service stopped");
    }

    private async Task CleanupOldUploadsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting cleanup of old upload blobs");

        try
        {
            var uploadContainer = _blobServiceClient.GetBlobContainerClient(AspireConstants.BlobUploadContainerName);
            
            // Check if container exists
            var containerExists = await uploadContainer.ExistsAsync(cancellationToken);
            if (!containerExists.Value)
            {
                _logger.LogInformation("Upload container does not exist, skipping cleanup");
                return;
            }

            var cutoffTime = DateTimeOffset.UtcNow.Subtract(_blobMaxAge);
            var deletedCount = 0;
            var totalCount = 0;

            // List all blobs in the upload container
            await foreach (var blobItem in uploadContainer.GetBlobsAsync(
                traits: BlobTraits.Metadata,
                cancellationToken: cancellationToken))
            {
                totalCount++;

                // Check if blob is older than the cutoff time
                if (blobItem.Properties.CreatedOn.HasValue && 
                    blobItem.Properties.CreatedOn.Value < cutoffTime)
                {
                    try
                    {
                        var blobClient = uploadContainer.GetBlobClient(blobItem.Name);
                        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                        
                        deletedCount++;
                        _logger.LogDebug("Deleted old upload blob: {BlobName}, Created: {CreatedOn}", 
                            blobItem.Name, blobItem.Properties.CreatedOn);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete blob: {BlobName}", blobItem.Name);
                    }
                }
            }

            _logger.LogInformation(
                "Upload cleanup completed. Processed {TotalCount} blobs, deleted {DeletedCount} old blobs (older than {CutoffTime})",
                totalCount, deletedCount, cutoffTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during upload cleanup process");
            throw;
        }
    }
}
