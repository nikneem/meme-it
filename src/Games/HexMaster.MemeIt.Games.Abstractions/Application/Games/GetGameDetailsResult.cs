using System;
using System.Collections.Generic;

namespace HexMaster.MemeIt.Games.Abstractions.Application.Games;

/// <summary>
/// Result containing full game details.
/// </summary>
/// <param name="GameCode">The game code.</param>
/// <param name="State">The current game state.</param>
/// <param name="CreatedAt">When the game was created.</param>
/// <param name="Players">Collection of players in the game.</param>
/// <param name="Rounds">Collection of rounds played.</param>
/// <param name="IsAdmin">Whether the requesting player is the admin.</param>
public sealed record GetGameDetailsResult(
    string GameCode,
    string State,
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<GamePlayerDto> Players,
    IReadOnlyCollection<GameRoundDto> Rounds,
    bool IsAdmin);

/// <summary>
/// Player details within a game.
/// </summary>
/// <param name="PlayerId">The player's unique identifier.</param>
/// <param name="DisplayName">The player's display name.</param>
/// <param name="IsReady">Whether the player is ready (lobby state).</param>
public sealed record GamePlayerDto(Guid PlayerId, string DisplayName, bool IsReady);

/// <summary>
/// Round details within a game.
/// </summary>
/// <param name="RoundNumber">The round number.</param>
/// <param name="SubmissionCount">Number of submissions received.</param>
public sealed record GameRoundDto(int RoundNumber, int SubmissionCount);
