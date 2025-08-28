using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace HexMaster.MemeIt.Aspire.AppHost.Services;

public class BlobSeeder
{
    private readonly ILogger _logger;
    private readonly string _containerName;
    private readonly Dictionary<string, string> _mimeTypeMap = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".webp", "image/webp" },
        { ".mp4", "video/mp4" }
    };

    public BlobSeeder(ILogger logger, string containerName)
    {
        _logger = logger;
        _containerName = containerName;
    }

    public async Task SeedEmbeddedResourcesAsync(BlobServiceClient blobServiceClient, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting to upload embedded meme resources to container '{ContainerName}'...", _containerName);

        try
        {
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var assembly = Assembly.GetExecutingAssembly();
            var supportedExtensions = _mimeTypeMap.Keys.ToArray();

            // Get all embedded resource names that match our supported extensions
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(name => supportedExtensions.Any(ext => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            _logger.LogInformation("Found {ResourceCount} embedded resources to process", resourceNames.Count);

            var uploadedCount = 0;
            var skippedCount = 0;

            foreach (var resourceName in resourceNames)
            {
                try
                {
                    var result = await ProcessEmbeddedResourceAsync(assembly, containerClient, resourceName, cancellationToken);
                    
                    if (result.Uploaded)
                    {
                        uploadedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process embedded resource: {ResourceName}", resourceName);
                }
            }

            _logger.LogInformation("Completed seeding process. Uploaded: {UploadedCount}, Skipped: {SkippedCount}", 
                uploadedCount, skippedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the seeding process");
            throw;
        }
    }

    private async Task<(bool Uploaded, string FileName)> ProcessEmbeddedResourceAsync(
        Assembly assembly, 
        BlobContainerClient containerClient, 
        string resourceName, 
        CancellationToken cancellationToken)
    {
        // Extract the filename from the resource name
        var fileName = ExtractFileNameFromResourceName(resourceName);
        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        // Check if blob already exists
        var blobClient = containerClient.GetBlobClient(fileName);
        var existsResponse = await blobClient.ExistsAsync(cancellationToken);

        if (existsResponse.Value)
        {
            _logger.LogDebug("Blob '{FileName}' already exists, skipping", fileName);
            return (false, fileName);
        }

        // Read the embedded resource
        await using var resourceStream = assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            _logger.LogWarning("Could not read embedded resource: {ResourceName}", resourceName);
            return (false, fileName);
        }

        // Get the correct MIME type
        var mimeType = GetMimeType(fileExtension);

        // Prepare blob upload options with correct content type
        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = mimeType
            }
        };

        try
        {
            // Upload the resource to blob storage with proper MIME type
            await blobClient.UploadAsync(resourceStream, uploadOptions, cancellationToken);
            _logger.LogInformation("Successfully uploaded embedded resource: {FileName} with content type: {ContentType}", 
                fileName, mimeType);
            
            return (true, fileName);
        }
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "BlobAlreadyExists")
        {
            _logger.LogDebug("Blob '{FileName}' was created by another process during upload, skipping", fileName);
            return (false, fileName);
        }
    }

    private string ExtractFileNameFromResourceName(string resourceName)
    {
        // Resource names are typically in format: Namespace.Folder.Filename.Extension
        // We want to extract just the Filename.Extension part
        var nameParts = resourceName.Split('.');
        return string.Join(".", nameParts.Skip(Math.Max(0, nameParts.Length - 2)));
    }

    private string GetMimeType(string fileExtension)
    {
        return _mimeTypeMap.TryGetValue(fileExtension, out var mimeType) 
            ? mimeType 
            : "application/octet-stream";
    }
}