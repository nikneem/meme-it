using HexMaster.MemeIt.Games.Abstractions.Application.Games;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;
using HexMaster.MemeIt.Games.Abstractions.Repositories;

namespace HexMaster.MemeIt.Games.Application.Games;

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

        return new GetGameDetailsResult(
            game.GameCode,
            game.State.Name,
            game.CreatedAt,
            players,
            rounds,
            isAdmin);
    }
}
