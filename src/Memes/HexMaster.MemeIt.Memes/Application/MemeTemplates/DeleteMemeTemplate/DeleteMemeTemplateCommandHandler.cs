using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Configuration;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Repositories;
using Microsoft.Extensions.Options;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates.DeleteMemeTemplate;

/// <summary>
/// Handler for deleting a meme template.
/// </summary>
public class DeleteMemeTemplateCommandHandler : ICommandHandler<DeleteMemeTemplateCommand, DeleteMemeTemplateResult>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly BlobStorageOptions _options;

    public DeleteMemeTemplateCommandHandler(
        IMemeTemplateRepository repository,
        IBlobStorageService blobStorageService,
        IOptions<BlobStorageOptions> options)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<DeleteMemeTemplateResult> HandleAsync(
        DeleteMemeTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Get the template to retrieve the image URL
        var template = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (template == null)
        {
            return new DeleteMemeTemplateResult(false);
        }

        // Delete the template from the database
        await _repository.DeleteAsync(command.Id, cancellationToken);

        // Delete the associated blob
        try
        {
            var blobName = ExtractBlobNameFromUrl(template.ImageUrl);
            await _blobStorageService.DeleteBlobAsync(
                blobName,
                _options.MemesContainerName,
                cancellationToken);
        }
        catch (Exception)
        {
            // Log the error but don't fail the delete operation
            // The database record is already deleted
            // In a production system, you might want to queue this for retry
        }

        return new DeleteMemeTemplateResult(true);
    }

    private static string ExtractBlobNameFromUrl(string imageUrl)
    {
        // Image URL is a path like "/memes/blob.png"
        var lastSlashIndex = imageUrl.LastIndexOf('/');
        if (lastSlashIndex == -1)
        {
            throw new InvalidOperationException("Invalid image URL format");
        }

        return imageUrl.Substring(lastSlashIndex + 1);
    }
}
