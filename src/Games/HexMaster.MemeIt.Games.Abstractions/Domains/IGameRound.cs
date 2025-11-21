using System.Collections.Generic;

namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Describes the state of a single round within a game.
/// </summary>
public interface IGameRound
{
    /// <summary>
    /// Sequential round number starting at 1.
    /// </summary>
    int RoundNumber { get; }

    /// <summary>
    /// The date and time when this round started.
    /// </summary>
    DateTimeOffset StartedAt { get; }

    /// <summary>
    /// Per-player meme submissions collected for this round.
    /// </summary>
    IReadOnlyCollection<IMemeSubmission> Submissions { get; }
}
