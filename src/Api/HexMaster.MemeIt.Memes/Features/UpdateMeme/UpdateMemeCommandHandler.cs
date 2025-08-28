using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.DataTransferObjects;
using HexMaster.MemeIt.Memes.Models;
using HexMaster.MemeIt.Memes.Services;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.UpdateMeme;

public class UpdateMemeCommandHandler : ICommandHandler<UpdateMemeCommand, OperationResult<MemeTemplateResponse>>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobUrlService _blobUrlService;

    public UpdateMemeCommandHandler(IMemeTemplateRepository repository, IBlobUrlService blobUrlService)
    {
        _repository = repository;
        _blobUrlService = blobUrlService;
    }

    public async ValueTask<OperationResult<MemeTemplateResponse>> HandleAsync(UpdateMemeCommand command, CancellationToken cancellationToken)
    {
        var existingTemplate = await _repository.GetByIdAsync(command.Id, cancellationToken);
        
        if (existingTemplate == null)
        {
            return new OperationResult<MemeTemplateResponse>(false, null);
        }

        // Create text areas using the domain model constructor
        var textAreas = command.TextAreas.Select(ta => new TextArea(
            ta.X, ta.Y, ta.Width, ta.Height, ta.FontFamily, ta.FontSize, 
            ta.FontColor, ta.FontBold, ta.MaxLength, ta.BorderThickness, ta.BorderColor)).ToArray();

        // Update the template using the domain model update method
        existingTemplate.Update(
            command.Name,
            command.Description,
            existingTemplate.SourceImageUrl, // Keep the same source image filename
            existingTemplate.SourceWidth,    // Keep the same dimensions
            existingTemplate.SourceHeight,
            textAreas);

        var updatedTemplate = await _repository.UpdateAsync(existingTemplate, cancellationToken);
        
        // Construct full URL for response
        var fullImageUrl = _blobUrlService.GetMemeImageUrl(updatedTemplate.SourceImageUrl);
        
        var response = new MemeTemplateResponse(
            updatedTemplate.Id,
            updatedTemplate.Name,
            updatedTemplate.Description,
            fullImageUrl,
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
