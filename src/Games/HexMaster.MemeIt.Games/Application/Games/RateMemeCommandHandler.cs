using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using Microsoft.Extensions.Logging;

namespace HexMaster.MemeIt.Games.Application.Games;

/// <summary>
/// Handles rating a meme in a game round.
/// </summary>
public sealed class RateMemeCommandHandler : ICommandHandler<RateMemeCommand, RateMemeResult>
{
    private readonly IGamesRepository _repository;
    private readonly ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult> _endScorePhaseHandler;
    private readonly ILogger<RateMemeCommandHandler> _logger;

    public RateMemeCommandHandler(
        IGamesRepository repository,
        ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult> endScorePhaseHandler,
        ILogger<RateMemeCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _endScorePhaseHandler = endScorePhaseHandler ?? throw new ArgumentNullException(nameof(endScorePhaseHandler));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RateMemeResult> HandleAsync(
        RateMemeCommand command,
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

        try
        {
            // AddScore will validate: rating range (0-5), no self-voting, and store the score
            // It allows duplicate votes (last one wins) but we'll check if already scored to ignore
            var existingScores = round.GetScoresForMeme(command.MemeId);
            if (existingScores.ContainsKey(command.PlayerId))
            {
                _logger.LogInformation(
                    "Player {PlayerId} already rated meme {MemeId} in round {RoundNumber} of game {GameCode}. Ignoring.",
                    command.PlayerId, command.MemeId, command.RoundNumber, command.GameCode);
                return new RateMemeResult(command.GameCode, command.RoundNumber, false);
            }

            game.AddScore(command.RoundNumber, command.MemeId, command.PlayerId, command.Rating);

            await _repository.UpdateAsync(game, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Player {PlayerId} rated meme {MemeId} with score {Rating} in round {RoundNumber} of game {GameCode}.",
                command.PlayerId, command.MemeId, command.Rating, command.RoundNumber, command.GameCode);

            // Check if all eligible players have rated this meme
            var updatedScores = round.GetScoresForMeme(command.MemeId);
            var memeOwner = round.Submissions.FirstOrDefault(s => s.MemeTemplateId == command.MemeId)?.PlayerId;
            var eligibleVoters = game.Players.Count(p => p.PlayerId != memeOwner); // All players except the meme creator
            var totalRatings = updatedScores.Count;

            if (totalRatings >= eligibleVoters)
            {
                _logger.LogInformation(
                    "All {EligibleVoters} eligible players have rated meme {MemeId} in round {RoundNumber} of game {GameCode}. Ending score phase.",
                    eligibleVoters, command.MemeId, command.RoundNumber, command.GameCode);

                // Automatically end the score phase for this meme
                var endScorePhaseCommand = new EndScorePhaseCommand(command.GameCode, command.RoundNumber, command.MemeId);
                await _endScorePhaseHandler.HandleAsync(endScorePhaseCommand, cancellationToken).ConfigureAwait(false);
            }

            return new RateMemeResult(command.GameCode, command.RoundNumber, true);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "Failed to add score for player {PlayerId} on meme {MemeId} in round {RoundNumber} of game {GameCode}.",
                command.PlayerId, command.MemeId, command.RoundNumber, command.GameCode);
            return new RateMemeResult(command.GameCode, command.RoundNumber, false);
        }
    }
}
