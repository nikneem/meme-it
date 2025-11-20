using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.IntegrationEvents.Publishers;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles removing a player from a game, ensuring only the admin can perform this action.
/// </summary>
public sealed class RemovePlayerCommandHandler : ICommandHandler<RemovePlayerCommand, RemovePlayerResult>
{
    private readonly IGamesRepository _repository;
    private readonly IIntegrationEventPublisher? _publisher;

    public RemovePlayerCommandHandler(IGamesRepository repository, IIntegrationEventPublisher? publisher = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _publisher = publisher;
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

        // Capture player info before removal
        var player = game.Players.FirstOrDefault(p => p.PlayerId == command.PlayerIdToRemove);
        string? displayName = player?.DisplayName;

        // Domain model validates that admin cannot be removed
        game.RemovePlayer(command.PlayerIdToRemove);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Publish integration event if available and player existed
        if (player is not null && _publisher is not null)
        {
            var @event = new HexMaster.MemeIt.IntegrationEvents.Events.PlayerRemovedEvent(player.PlayerId, displayName!, game.GameCode);
            await _publisher.PublishPlayerRemovedAsync(@event, cancellationToken).ConfigureAwait(false);
        }

        return new RemovePlayerResult();
    }
}
