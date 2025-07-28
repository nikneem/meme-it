namespace HexMaster.MemeIt.Memes.Abstractions;

public interface IBlobStorageService
{
    Task<string> MoveFromUploadToMemesAsync(string sourceFileName, CancellationToken cancellationToken = default);
    Task DeleteFromMemesAsync(string fileName, CancellationToken cancellationToken = default);
}
