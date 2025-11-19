using System;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles removing a player from a game, ensuring only the admin can perform this action.
/// </summary>
public sealed class RemovePlayerCommandHandler : ICommandHandler<RemovePlayerCommand, RemovePlayerResult>
{
    private readonly IGamesRepository _repository;

    public RemovePlayerCommandHandler(IGamesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<RemovePlayerResult> HandleAsync(RemovePlayerCommand command, CancellationToken cancellationToken = default)
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

        // Verify caller is the admin
        if (game.AdminPlayerId != command.AdminPlayerId)
        {
            throw new UnauthorizedAccessException("Only the game admin can remove players.");
        }

        // Domain model validates that admin cannot be removed
        game.RemovePlayer(command.PlayerIdToRemove);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        return new RemovePlayerResult();
    }
}
