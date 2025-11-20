using System;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles joining an existing game by fetching the game, adding the player, and persisting.
/// </summary>
public sealed class JoinGameCommandHandler : ICommandHandler<JoinGameCommand, JoinGameResult>
{
    private readonly IGamesRepository _repository;
    private readonly HexMaster.MemeIt.Games.Application.Integration.IIntegrationEventPublisher? _publisher;

    public JoinGameCommandHandler(IGamesRepository repository, HexMaster.MemeIt.Games.Application.Integration.IIntegrationEventPublisher? publisher = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _publisher = publisher;
    }

    public async Task<JoinGameResult> HandleAsync(JoinGameCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.GameCode))
        {
            throw new ArgumentException("Game code is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.PlayerName))
        {
            throw new ArgumentException("Player name is required.", nameof(command));
        }

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game is null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        // Domain model validates password and state
        game.AddPlayer(command.PlayerId, command.PlayerName, command.Password);

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Publish integration event
        if (_publisher is not null)
        {
            var @event = new HexMaster.MemeIt.IntegrationEvents.Events.PlayerJoinedEvent(
                command.PlayerId,
                command.PlayerName,
                game.GameCode);
            await _publisher.PublishPlayerJoinedAsync(@event, cancellationToken).ConfigureAwait(false);
        }

        return new JoinGameResult(game.GameCode, command.PlayerId, game.State);
    }
}
