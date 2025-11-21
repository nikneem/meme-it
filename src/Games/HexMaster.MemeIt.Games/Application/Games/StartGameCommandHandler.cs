using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents.Events;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles the StartGameCommand by verifying admin rights, starting the game, and publishing the event.
/// </summary>
public sealed class StartGameCommandHandler : ICommandHandler<StartGameCommand, StartGameResult>
{
    private readonly IGamesRepository _repository;
    private readonly DaprClient _daprClient;

    public StartGameCommandHandler(
        IGamesRepository repository,
        DaprClient daprClient)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
    }

    public async Task<StartGameResult> HandleAsync(StartGameCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        // Verify the caller is the game admin
        if (game.AdminPlayerId != command.AdminPlayerId)
        {
            throw new UnauthorizedAccessException("Only the game admin can start the game.");
        }

        // Start the first round (this also sets State to InProgress and updates CurrentRound)
        var round = game.NextRound();

        // Persist the updated game
        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Publish GameStarted event
        var gameStartedEvent = new GameStartedEvent(game.GameCode, round.RoundNumber);
        await _daprClient.PublishEventAsync(
            "chatservice-pubsub",
            "gamestarted",
            gameStartedEvent,
            cancellationToken).ConfigureAwait(false);

        return new StartGameResult(game.GameCode, round.RoundNumber);
    }
}
