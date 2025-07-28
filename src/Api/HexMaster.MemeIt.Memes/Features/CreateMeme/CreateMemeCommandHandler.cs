using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.Models;
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

        // Create meme template
        var memeTemplate = new MemeTemplate
        {
            Name = command.Name,
            Description = command.Description,
            SourceImageUrl = sourceImageUrl,
            SourceWidth = command.SourceWidth,
            SourceHeight = command.SourceHeight,
            TextAreas = command.TextAreas.Select(ta => new TextArea
            {
                X = ta.X,
                Y = ta.Y,
                Width = ta.Width,
                Height = ta.Height,
                FontFamily = ta.FontFamily,
                FontSize = ta.FontSize,
                FontColor = ta.FontColor,
                MaxLength = ta.MaxLength
            }).ToArray()
        };

        var createdTemplate = await _repository.CreateAsync(memeTemplate, cancellationToken);
        
        return new CreateMemeResponse(createdTemplate.Id, createdTemplate.SourceImageUrl);
    }
}
