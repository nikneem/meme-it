using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Configuration;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;
using Microsoft.Extensions.Options;

namespace HexMaster.MemeIt.Memes.Application.MemeTemplates.CreateMemeTemplate;

/// <summary>
/// Handler for creating a new meme template.
/// </summary>
public class CreateMemeTemplateCommandHandler : ICommandHandler<CreateMemeTemplateCommand, CreateMemeTemplateResult>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly BlobStorageOptions _options;

    public CreateMemeTemplateCommandHandler(
        IMemeTemplateRepository repository,
        IBlobStorageService blobStorageService,
        IOptions<BlobStorageOptions> options)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<CreateMemeTemplateResult> HandleAsync(
        CreateMemeTemplateCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        try
        {
            // Extract blob name from image URL
            var blobName = ExtractBlobNameFromUrl(command.ImageUrl);

            // Move blob from upload container to memes container
            var destinationBlobUrl = await _blobStorageService.MoveBlobAsync(
                blobName,
                _options.UploadContainerName,
                _options.MemesContainerName,
                cancellationToken);

            // Extract the path from the destination blob URL for storage
            var imageUrl = new Uri(destinationBlobUrl).PathAndQuery;

            // Map DTOs to value objects
            var textAreas = command.TextAreas.Select(dto =>
                TextAreaDefinition.Create(
                    dto.X,
                    dto.Y,
                    dto.Width,
                    dto.Height,
                    dto.FontSize,
                    dto.FontColor,
                    dto.BorderSize,
                    dto.BorderColor,
                    dto.IsBold
                )
            ).ToList();

            // Create domain entity with the new image URL
            var template = MemeTemplate.Create(command.Title, imageUrl, command.Width, command.Height, textAreas);

            // Persist
            var id = await _repository.AddAsync(template, cancellationToken);

            return new CreateMemeTemplateResult(id);
        }
        catch (DomainException ex)
        {
            // In a real application, you might want to handle domain exceptions differently
            // For now, let them bubble up to be caught by the API layer
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    private static string ExtractBlobNameFromUrl(string imageUrl)
    {
        // Image URL can be either full URL or path
        // e.g., "https://storage.blob.core.windows.net/upload/blob.png" or "/upload/blob.png"
        var lastSlashIndex = imageUrl.LastIndexOf('/');
        if (lastSlashIndex == -1)
        {
            throw new InvalidOperationException("Invalid image URL format");
        }

        return imageUrl.Substring(lastSlashIndex + 1);
    }
}
