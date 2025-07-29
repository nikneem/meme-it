using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.Models;
using HexMaster.MemeIt.Memes.Models.Factories;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.CreateMeme;

public class CreateMemeCommandHandler : ICommandHandler<CreateMemeCommand, CreateMemeResponse>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobStorageService _blobStorageService;

    public CreateMemeCommandHandler(IMemeTemplateRepository repository, IBlobStorageService blobStorageService)
    {
        _repository = repository;
        _blobStorageService = blobStorageService;
    }

    public async ValueTask<CreateMemeResponse> HandleAsync(CreateMemeCommand command, CancellationToken cancellationToken)
    {
        // Move blob from upload to memes container
        var sourceImageUrl = await _blobStorageService.MoveFromUploadToMemesAsync(command.SourceImage, cancellationToken);

        // Create text areas using the domain model constructor
        var textAreas = command.TextAreas.Select(ta => new TextArea(
            ta.X, ta.Y, ta.Width, ta.Height, ta.FontFamily, ta.FontSize, 
            ta.FontColor, ta.FontBold, ta.MaxLength, ta.BorderThickness, ta.BorderColor)).ToArray();

        // Create meme template using the domain model constructor
        var memeTemplate = new MemeTemplate(
            command.Name,
            command.Description,
            sourceImageUrl,
            command.SourceWidth,
            command.SourceHeight,
            textAreas);

        var createdTemplate = await _repository.CreateAsync(memeTemplate, cancellationToken);
        
        return new CreateMemeResponse(createdTemplate.Id, createdTemplate.SourceImageUrl);
    }
}
