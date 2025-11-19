using System;
using HexMaster.MemeIt.Games.Abstractions.Domains;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Represents a player participating in a Meme-It game.
/// </summary>
public sealed class GamePlayer : IGamePlayer
{
    public GamePlayer(Guid playerId, string displayName)
    {
        PlayerId = playerId != Guid.Empty
            ? playerId
            : throw new ArgumentException("Player id must be provided", nameof(playerId));

        DisplayName = !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : throw new ArgumentException("Display name must be provided", nameof(displayName));
    }

    public Guid PlayerId { get; }

    public string DisplayName { get; }
}
