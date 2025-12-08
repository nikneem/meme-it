namespace HexMaster.MemeIt.Memes.Abstractions.Configuration;

/// <summary>
/// Configuration options for Azure Blob Storage containers.
/// </summary>
public class BlobStorageOptions
{
    /// <summary>
    /// Configuration section name for blob storage options.
    /// </summary>
    public const string SectionName = "BlobStorage";

    /// <summary>
    /// Name of the container for temporary uploads before validation.
    /// Default: "upload"
    /// </summary>
    public string UploadContainerName { get; set; } = "upload";

    /// <summary>
    /// Name of the container for finalized meme templates.
    /// Default: "memes"
    /// </summary>
    public string MemesContainerName { get; set; } = "memes";
}
