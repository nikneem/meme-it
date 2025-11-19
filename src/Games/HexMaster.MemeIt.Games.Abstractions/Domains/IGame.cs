using System;
using System.Collections.Generic;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;

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

    /// <summary>
    /// Adds a player to the game lobby, enforcing password rules when required.
    /// </summary>
    /// <param name="playerId">Unique identifier of the player.</param>
    /// <param name="displayName">Display name visible to other players.</param>
    /// <param name="passwordAttempt">Password supplied by the joining player when the game is protected.</param>
    void AddPlayer(Guid playerId, string displayName, string? passwordAttempt = null);

    /// <summary>
    /// Removes a player and any submissions they have contributed.
    /// </summary>
    /// <param name="playerId">Identifier of the player to remove.</param>
    void RemovePlayer(Guid playerId);

    /// <summary>
    /// Records a meme submission for the specified round.
    /// </summary>
    /// <param name="roundNumber">Round the submission belongs to.</param>
    /// <param name="submission">Submission payload captured from the player.</param>
    void AddMemeSubmission(int roundNumber, IMemeSubmission submission);

    /// <summary>
    /// Creates or advances to the next round, resetting the lifecycle to the creative phase.
    /// </summary>
    /// <returns>The round that became active.</returns>
    IGameRound NextRound();

    /// <summary>
    /// Attempts to move the game into the requested state, enforcing the state machine rules.
    /// </summary>
    /// <param name="targetState">State to transition to.</param>
    void ChangeState(GameState targetState);

    /// <summary>
    /// Completes the game, finalizing leaderboards and locking further changes.
    /// </summary>
    void Finish();
}
