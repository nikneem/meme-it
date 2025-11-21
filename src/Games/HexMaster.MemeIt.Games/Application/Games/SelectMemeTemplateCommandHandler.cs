using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles the SelectMemeTemplateCommand by creating or updating a submission with the selected meme template.
/// </summary>
public sealed class SelectMemeTemplateCommandHandler : ICommandHandler<SelectMemeTemplateCommand, SelectMemeTemplateResult>
{
    private readonly IGamesRepository _repository;

    public SelectMemeTemplateCommandHandler(IGamesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SelectMemeTemplateResult> HandleAsync(SelectMemeTemplateCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        // Verify player is part of the game
        if (!game.Players.Any(p => p.PlayerId == command.PlayerId))
        {
            throw new InvalidOperationException("Player is not part of this game.");
        }

        // Create a submission with the selected meme template (empty text entries for now)
        var submission = new MemeSubmission(command.PlayerId, command.MemeTemplateId, Array.Empty<MemeTextEntry>());

        game.AddMemeSubmission(command.RoundNumber, submission);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        return new SelectMemeTemplateResult(game.GameCode, command.PlayerId, command.RoundNumber, command.MemeTemplateId);
    }
}
