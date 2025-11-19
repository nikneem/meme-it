using System;

namespace HexMaster.MemeIt.Games.Abstractions.Domains;

/// <summary>
/// Read-only projection of a player participating in a game.
/// </summary>
public interface IGamePlayer
{
    Guid PlayerId { get; }
    string DisplayName { get; }
}
