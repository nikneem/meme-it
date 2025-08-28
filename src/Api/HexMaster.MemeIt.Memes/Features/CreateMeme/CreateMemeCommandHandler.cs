using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.Models;
using HexMaster.MemeIt.Memes.Models.Factories;
using HexMaster.MemeIt.Memes.Services;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.CreateMeme;

public class CreateMemeCommandHandler : ICommandHandler<CreateMemeCommand, CreateMemeResponse>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IBlobUrlService _blobUrlService;

    public CreateMemeCommandHandler(IMemeTemplateRepository repository, IBlobStorageService blobStorageService, IBlobUrlService blobUrlService)
    {
        _repository = repository;
        _blobStorageService = blobStorageService;
        _blobUrlService = blobUrlService;
    }

    public async ValueTask<CreateMemeResponse> HandleAsync(CreateMemeCommand command, CancellationToken cancellationToken)
    {
        // Move blob from upload to memes container - now returns filename only
        var sourceImageFilename = await _blobStorageService.MoveFromUploadToMemesAsync(command.SourceImage, cancellationToken);

        // Create text areas using the domain model constructor
        var textAreas = command.TextAreas.Select(ta => new TextArea(
            ta.X, ta.Y, ta.Width, ta.Height, ta.FontFamily, ta.FontSize, 
            ta.FontColor, ta.FontBold, ta.MaxLength, ta.BorderThickness, ta.BorderColor)).ToArray();

        // Create meme template using the domain model constructor with filename only
        var memeTemplate = new MemeTemplate(
            command.Name,
            command.Description,
            sourceImageFilename,
            command.SourceWidth,
            command.SourceHeight,
            textAreas);

        var createdTemplate = await _repository.CreateAsync(memeTemplate, cancellationToken);
        
        // Construct full URL for the response
        var fullImageUrl = _blobUrlService.GetMemeImageUrl(createdTemplate.SourceImageUrl);
        
        return new CreateMemeResponse(createdTemplate.Id, fullImageUrl);
    }
}
