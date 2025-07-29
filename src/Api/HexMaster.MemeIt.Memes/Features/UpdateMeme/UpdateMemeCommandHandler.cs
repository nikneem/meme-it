using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using HexMaster.MemeIt.Memes.Models;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.UpdateMeme;

public class UpdateMemeCommandHandler : ICommandHandler<UpdateMemeCommand, OperationResult<MemeTemplateResponse>>
{
    private readonly IMemeTemplateRepository _repository;

    public UpdateMemeCommandHandler(IMemeTemplateRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<OperationResult<MemeTemplateResponse>> HandleAsync(UpdateMemeCommand command, CancellationToken cancellationToken)
    {
        var existingTemplate = await _repository.GetByIdAsync(command.Id, cancellationToken);
        
        if (existingTemplate == null)
        {
            return new OperationResult<MemeTemplateResponse>(false, null);
        }

        existingTemplate.Name = command.Name;
        existingTemplate.Description = command.Description;
        existingTemplate.TextAreas = command.TextAreas.Select(ta => new TextArea
        {
            X = ta.X,
            Y = ta.Y,
            Width = ta.Width,
            Height = ta.Height,
            FontFamily = ta.FontFamily,
            FontSize = ta.FontSize,
            FontColor = ta.FontColor,
            MaxLength = ta.MaxLength
        }).ToArray();

        var updatedTemplate = await _repository.UpdateAsync(existingTemplate, cancellationToken);
        
        var response = new MemeTemplateResponse(
            updatedTemplate.Id,
            updatedTemplate.Name,
            updatedTemplate.Description,
            updatedTemplate.SourceImageUrl,
            updatedTemplate.SourceWidth,
            updatedTemplate.SourceHeight,
            updatedTemplate.TextAreas.Select(ta => new MemeTextArea(
                ta.X, ta.Y, ta.Width, ta.Height,
                ta.FontFamily, ta.FontSize, ta.FontColor, ta.FontBold, ta.MaxLength, ta.BorderThickness, ta.BorderColor)).ToArray(),
            updatedTemplate.CreatedAt,
            updatedTemplate.UpdatedAt);

        return new OperationResult<MemeTemplateResponse>(true, response);
    }
}
