namespace HexMaster.MemeIt.Memes.Abstractions;

public interface ISasTokenService
{
    Task<string> GenerateUploadSasTokenAsync(string fileName, CancellationToken cancellationToken = default);
}
