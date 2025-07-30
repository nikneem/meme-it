using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using HexMaster.MemeIt.Memes.Abstractions;

namespace HexMaster.MemeIt.Memes.Services;

public class SasTokenService : ISasTokenService
{
    private readonly BlobServiceClient _blobServiceClient;

    public SasTokenService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> GenerateUploadSasTokenAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var uploadContainer = _blobServiceClient.GetBlobContainerClient(AspireConstants.BlobUploadContainerName);
        
        // Ensure upload container exists
        await uploadContainer.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        
        var blobClient = uploadContainer.GetBlobClient(fileName);
        
        // Check if the blob client can generate SAS tokens
        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("BlobClient cannot generate SAS tokens. Ensure you're using account key authentication.");
        }
        
        // Create SAS token that expires in 10 minutes
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = AspireConstants.BlobUploadContainerName,
            BlobName = fileName,
            Resource = "b", // blob
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10)
        };
        
        // Grant write permissions for upload
        sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write);
        
        // Generate the SAS URI
        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        
        return sasUri.ToString();
    }
}
