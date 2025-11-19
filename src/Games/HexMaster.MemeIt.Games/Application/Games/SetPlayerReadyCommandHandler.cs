using System;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles setting a player's ready state in the lobby.
/// </summary>
public sealed class SetPlayerReadyCommandHandler : ICommandHandler<SetPlayerReadyCommand, SetPlayerReadyResult>
{
    private readonly IGamesRepository _repository;

    public SetPlayerReadyCommandHandler(IGamesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SetPlayerReadyResult> HandleAsync(SetPlayerReadyCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.GameCode))
        {
            throw new ArgumentException("Game code is required.", nameof(command));
        }

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game is null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        // Domain model validates state and player membership
        game.SetPlayerReady(command.PlayerId, command.IsReady);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        return new SetPlayerReadyResult(command.PlayerId, command.IsReady, game.AreAllPlayersReady());
    }
}
