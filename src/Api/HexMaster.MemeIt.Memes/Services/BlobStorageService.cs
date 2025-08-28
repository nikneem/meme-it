using Azure.Storage.Blobs;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Aspire;

namespace HexMaster.MemeIt.Memes.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<string> MoveFromUploadToMemesAsync(string sourceFileName, CancellationToken cancellationToken = default)
    {
        var uploadContainer = _blobServiceClient.GetBlobContainerClient(AspireConstants.BlobUploadContainerName);
        var memesContainer = _blobServiceClient.GetBlobContainerClient(AspireConstants.BlobMemesContainerName);
        
        // Ensure containers exist
        await uploadContainer.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await memesContainer.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        
        var sourceBlobClient = uploadContainer.GetBlobClient(sourceFileName);
        
        // Check if source blob exists
        var exists = await sourceBlobClient.ExistsAsync(cancellationToken);
        if (!exists.Value)
        {
            throw new FileNotFoundException($"Source file '{sourceFileName}' not found in upload container.");
        }
        
        // Generate new filename for memes container
        var fileExtension = Path.GetExtension(sourceFileName);
        var newFileName = $"{Guid.NewGuid()}{fileExtension}";
        var destinationBlobClient = memesContainer.GetBlobClient(newFileName);
        
        // Copy blob from upload to memes container
        var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri, cancellationToken: cancellationToken);
        await copyOperation.WaitForCompletionAsync(cancellationToken);
        
        // Delete from upload container
        await sourceBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        
        // Return only the filename instead of full URL
        return newFileName;
    }

    public async Task DeleteFromMemesAsync(string fileName, CancellationToken cancellationToken = default)
    {
        var memesContainer = _blobServiceClient.GetBlobContainerClient(AspireConstants.BlobMemesContainerName);
        var blobClient = memesContainer.GetBlobClient(Path.GetFileName(fileName));
        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
