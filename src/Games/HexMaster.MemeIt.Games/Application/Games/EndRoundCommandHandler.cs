using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.IntegrationEvents;
using HexMaster.MemeIt.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles ending a round, calculating scoreboard, and determining next steps.
/// </summary>
public sealed class EndRoundCommandHandler : ICommandHandler<EndRoundCommand, EndRoundResult>
{
    private readonly IGamesRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly IScheduledTaskService _scheduledTaskService;
    private readonly ILogger<EndRoundCommandHandler> _logger;

    public EndRoundCommandHandler(
        IGamesRepository repository,
        DaprClient daprClient,
        IScheduledTaskService scheduledTaskService,
        ILogger<EndRoundCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _daprClient = daprClient ?? throw new ArgumentNullException(nameof(daprClient));
        _scheduledTaskService = scheduledTaskService ?? throw new ArgumentNullException(nameof(scheduledTaskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EndRoundResult> HandleAsync(
        EndRoundCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var game = await _repository.GetByGameCodeAsync(command.GameCode, cancellationToken).ConfigureAwait(false);
        if (game == null)
        {
            throw new InvalidOperationException($"Game with code '{command.GameCode}' not found.");
        }

        var round = game.GetRound(command.RoundNumber);
        if (round == null)
        {
            throw new InvalidOperationException($"Round {command.RoundNumber} not found in game {command.GameCode}.");
        }
        bool isLastRound = game.CurrentRound >= game.RoundTarget;

        // Check if round has already ended (idempotency check)
        if (round.HasRoundEnded)
        {
            _logger.LogInformation(
                "Round {RoundNumber} of game {GameCode} has already ended. Ignoring duplicate request.",
                command.RoundNumber, game.GameCode);

            return new EndRoundResult(game.GameCode, command.RoundNumber, false, isLastRound, false);
        }

        await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

        // Calculate scoreboard: sum all ratings received by each player across all rounds
        var scoreboard = CalculateScoreboard(game);

        // Publish RoundEndedEvent with scoreboard
        var roundEndedEvent = new RoundEndedEvent(
            game.GameCode,
            command.RoundNumber,
            game.RoundTarget,
            scoreboard);

        await _daprClient.PublishEventAsync(
            DaprConstants.PubSubName,
            DaprConstants.Topics.RoundEnded,
            roundEndedEvent,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Round {RoundNumber} ended for game {GameCode}. Scoreboard published with {PlayerCount} players.",
            command.RoundNumber, game.GameCode, scoreboard.Count);

        if (!isLastRound)
        {
            // Schedule StartNewRound task for 10 seconds
            _scheduledTaskService.ScheduleStartNewRound(
                game.GameCode,
                game.CurrentRound + 1,
                delaySeconds: 10);

            _logger.LogInformation(
                "Game {GameCode} has more rounds to play ({Current}/{Target}). StartNewRound task scheduled.",
                game.GameCode, game.CurrentRound, game.RoundTarget);
        }
        else
        {
            _logger.LogInformation(
                "Game {GameCode} has completed all rounds ({Current}/{Target}). Game is complete.",
                game.GameCode, game.CurrentRound, game.RoundTarget);
        }


        return new EndRoundResult(game.GameCode, command.RoundNumber, true, isLastRound, true);
    }

    private static List<ScoreboardEntryDto> CalculateScoreboard(IGame game)
    {
        // Initialize all players with 0 score
        var playerScores = game.Players.ToDictionary(
            p => p.PlayerId,
            p => new { Name = p.DisplayName, TotalScore = 0 });

        // Iterate through all rounds and sum up each player's submission scores
        foreach (var round in game.Rounds)
        {
            foreach (var submission in round.Submissions)
            {
                if (playerScores.ContainsKey(submission.PlayerId))
                {
                    var current = playerScores[submission.PlayerId];
                    playerScores[submission.PlayerId] = new
                    {
                        current.Name,
                        TotalScore = current.TotalScore + submission.TotalScore
                    };
                }
            }
        }

        // Convert to list and sort by score descending
        return playerScores
            .Select(kvp => new ScoreboardEntryDto(kvp.Key, kvp.Value.Name, kvp.Value.TotalScore))
            .OrderByDescending(entry => entry.TotalScore)
            .ToList();
    }
}
