using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.IntegrationEvents.Events;
using HexMaster.MemeIt.IntegrationEvents.Publishers;

namespace HexMaster.MemeIt.Games.Application.Games.SetPlayerReady;

/// <summary>
/// Handles setting a player's ready state in the lobby.
/// </summary>
public sealed class SetPlayerReadyCommandHandler : ICommandHandler<SetPlayerReadyCommand, SetPlayerReadyResult>
{
    private readonly IGamesRepository _repository;
    private readonly IIntegrationEventPublisher? _publisher;

    public SetPlayerReadyCommandHandler(IGamesRepository repository, IIntegrationEventPublisher? publisher = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _publisher = publisher;
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

        // Publish integration event if publisher available
        var player = game.Players.FirstOrDefault(p => p.PlayerId == command.PlayerId);
        if (player is not null && _publisher is not null)
        {
            var @event = new PlayerStateChangedEvent(player.PlayerId, player.DisplayName, player.IsReady, game.GameCode);
            await _publisher.PublishPlayerStateChangedAsync(@event, cancellationToken).ConfigureAwait(false);
        }

        return new SetPlayerReadyResult(command.PlayerId, command.IsReady, game.AreAllPlayersReady());
    }
}
