using System;
using System.Collections.Generic;

namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Contract describing state that must be exposed by a Meme-It game aggregate.
/// </summary>
public interface IGame
{
    /// <summary>
    /// Eight character code used by players to join the game lobby.
    /// </summary>
    string GameCode { get; }

    /// <summary>
    /// Optional password required to join protected games. Null when no password is enforced.
    /// </summary>
    string? Password { get; }

    /// <summary>
    /// Identifier of the player who controls settings and round progression.
    /// </summary>
    Guid AdminPlayerId { get; }

    /// <summary>
    /// Players currently associated with the game.
    /// </summary>
    IReadOnlyCollection<IGamePlayer> Players { get; }

    /// <summary>
    /// Rounds played (or being played) within the game.
    /// </summary>
    IReadOnlyCollection<IGameRound> Rounds { get; }

    /// <summary>
    /// Current lifecycle state of the game.
    /// </summary>
    GameState State { get; }

    /// <summary>
    /// Timestamp for when the game was created.
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}
