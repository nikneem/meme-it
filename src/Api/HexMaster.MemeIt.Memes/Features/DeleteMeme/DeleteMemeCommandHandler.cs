using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Memes.Abstractions;
using Localizr.Core.Abstractions.Cqrs;

namespace HexMaster.MemeIt.Memes.Features.DeleteMeme;

public class DeleteMemeCommandHandler : ICommandHandler<DeleteMemeCommand, OperationResult<object>>
{
    private readonly IMemeTemplateRepository _repository;
    private readonly IBlobStorageService _blobStorageService;

    public DeleteMemeCommandHandler(IMemeTemplateRepository repository, IBlobStorageService blobStorageService)
    {
        _repository = repository;
        _blobStorageService = blobStorageService;
    }

    public async ValueTask<OperationResult<object>> HandleAsync(DeleteMemeCommand command, CancellationToken cancellationToken)
    {
        var existingTemplate = await _repository.GetByIdAsync(command.Id, cancellationToken);
        
        if (existingTemplate == null)
        {
            return new OperationResult<object>(false, null);
        }

        // Delete from blob storage
        await _blobStorageService.DeleteFromMemesAsync(existingTemplate.SourceImageUrl, cancellationToken);
        
        // Delete from database
        await _repository.DeleteAsync(command.Id, cancellationToken);
        
        return new OperationResult<object>(true, new object());
    }
}
