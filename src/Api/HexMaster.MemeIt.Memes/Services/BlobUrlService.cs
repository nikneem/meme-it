using Azure.Storage.Blobs;
using HexMaster.MemeIt.Aspire;

namespace HexMaster.MemeIt.Memes.Services;

public interface IBlobUrlService
{
    string GetMemeImageUrl(string filename);
    string ExtractFilenameFromUrl(string url);
}

public class BlobUrlService : IBlobUrlService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobUrlService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public string GetMemeImageUrl(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return string.Empty;

        var containerClient = _blobServiceClient.GetBlobContainerClient(AspireConstants.BlobMemesContainerName);
        var blobClient = containerClient.GetBlobClient(filename);
        return blobClient.Uri.ToString();
    }

    public string ExtractFilenameFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        // If it's already just a filename (no protocol), return as is
        if (!url.Contains("://"))
            return url;

        try
        {
            var uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
        catch
        {
            // If URL parsing fails, assume it might be just a filename
            return url;
        }
    }
}