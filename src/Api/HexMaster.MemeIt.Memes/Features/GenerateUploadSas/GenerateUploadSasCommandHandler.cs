using HexMaster.MemeIt.Memes.Abstractions;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.GenerateUploadSas;

public class GenerateUploadSasCommandHandler : ICommandHandler<GenerateUploadSasCommand, GenerateUploadSasResponse>
{
    private readonly ISasTokenService _sasTokenService;

    public GenerateUploadSasCommandHandler(ISasTokenService sasTokenService)
    {
        _sasTokenService = sasTokenService;
    }

    public async ValueTask<GenerateUploadSasResponse> HandleAsync(GenerateUploadSasCommand command, CancellationToken cancellationToken)
    {
        // Generate a unique filename if not provided or sanitize the provided one
        var fileName = string.IsNullOrWhiteSpace(command.FileName) 
            ? $"{Guid.NewGuid()}.jpg" 
            : SanitizeFileName(command.FileName);

        // Ensure the filename has a unique prefix to avoid conflicts
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

        var sasUri = await _sasTokenService.GenerateUploadSasTokenAsync(uniqueFileName, cancellationToken);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(10);

        return new GenerateUploadSasResponse
        {
            SasUri = sasUri,
            FileName = uniqueFileName,
            ExpiresAt = expiresAt
        };
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove invalid characters and keep only the filename part
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Ensure it has an extension
        if (!Path.HasExtension(sanitized))
        {
            sanitized += ".jpg";
        }

        return sanitized;
    }
}
