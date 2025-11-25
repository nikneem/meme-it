using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles submitting a meme with text entries for a player in a round.
/// </summary>
public sealed class SubmitMemeCommandHandler : ICommandHandler<SubmitMemeCommand, SubmitMemeResult>
{
    private readonly IGamesRepository _repository;

    public SubmitMemeCommandHandler(IGamesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SubmitMemeResult> HandleAsync(
        SubmitMemeCommand command,
        CancellationToken cancellationToken = default)
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

        // Map DTOs to domain MemeTextEntry objects
        var textEntries = command.TextEntries
            .Select(dto => new MemeTextEntry(dto.TextFieldId, dto.Value))
            .ToArray();

        // Create submission with meme template and text entries
        var submission = new MemeSubmission(
            command.PlayerId,
            command.MemeTemplateId,
            textEntries);

        game.AddMemeSubmission(command.RoundNumber, submission);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        return new SubmitMemeResult(
            game.GameCode,
            command.PlayerId,
            command.RoundNumber,
            command.MemeTemplateId,
            textEntries.Length);
    }
}
