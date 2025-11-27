using HexMaster.MemeIt.Games.Abstractions.Application.Games;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;
using HexMaster.MemeIt.Games.Abstractions.Repositories;

namespace HexMaster.MemeIt.Games.Application.Games.GetGameDetails;

/// <summary>
/// Handles retrieval of game details by game code.
/// </summary>
public sealed class GetGameDetailsQueryHandler : IQueryHandler<GetGameDetailsQuery, GetGameDetailsResult>
{
    private readonly IGamesRepository _repository;

    public GetGameDetailsQueryHandler(IGamesRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<GetGameDetailsResult> HandleAsync(GetGameDetailsQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.GameCode))
        {
            throw new ArgumentException("Game code is required", nameof(query.GameCode));
        }

        var game = await _repository.GetByGameCodeAsync(query.GameCode, cancellationToken);
        if (game is null)
        {
            throw new InvalidOperationException($"Game with code '{query.GameCode}' not found.");
        }

        // Authorization: verify the requesting player is in the game
        if (!game.Players.Any(p => p.PlayerId == query.RequestingPlayerId))
        {
            throw new UnauthorizedAccessException("You are not authorized to view this game.");
        }

        var isAdmin = game.AdminPlayerId == query.RequestingPlayerId;

        var players = game.Players
            .Select(p => new GamePlayerDto(p.PlayerId, p.DisplayName, p.IsReady))
            .ToArray();

        var rounds = game.Rounds
            .Select(r => new GameRoundDto(r.RoundNumber, r.Submissions.Count))
            .ToArray();

        // Get current round information if game is in progress
        CurrentRoundInfoDto? currentRoundInfo = null;
        PlayerSubmissionDto? playerSubmission = null;

        if (game.CurrentRound > 0)
        {
            var currentRound = game.Rounds.FirstOrDefault(r => r.RoundNumber == game.CurrentRound);
            if (currentRound != null)
            {
                // Determine phase
                string phase;
                DateTimeOffset? creativePhaseEndTime = null;

                if (!currentRound.HasCreativePhaseEnded)
                {
                    phase = "Creative";
                    // Creative phase typically lasts 60 seconds from round start
                    creativePhaseEndTime = currentRound.StartedAt.AddSeconds(60);
                }
                else if (!currentRound.HasScorePhaseEnded)
                {
                    phase = "Scoring";
                    creativePhaseEndTime = currentRound.StartedAt.AddSeconds(60);
                }
                else
                {
                    phase = "Ended";
                    creativePhaseEndTime = currentRound.StartedAt.AddSeconds(60);
                }

                currentRoundInfo = new CurrentRoundInfoDto(
                    currentRound.RoundNumber,
                    currentRound.StartedAt,
                    phase,
                    creativePhaseEndTime);

                // Get player's submission for current round
                var submission = currentRound.Submissions.FirstOrDefault(s => s.PlayerId == query.RequestingPlayerId);
                if (submission != null)
                {
                    var textEntries = submission.TextEntries
                        .Select(te => new TextEntryDto(te.TextFieldId.ToString(), te.Value))
                        .ToArray();

                    playerSubmission = new PlayerSubmissionDto(
                        submission.MemeTemplateId.ToString(),
                        textEntries,
                        currentRound.StartedAt); // Use round start time as approximation
                }
            }
        }

        return new GetGameDetailsResult(
            game.GameCode,
            game.State.Name,
            game.CreatedAt,
            players,
            rounds,
            isAdmin,
            currentRoundInfo,
            playerSubmission);
    }
}
