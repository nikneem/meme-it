using System.ComponentModel.DataAnnotations;

namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request payload for joining an existing game.
/// </summary>
public sealed record JoinGameRequest
{
    [MaxLength(32)]
    public string? PlayerName { get; init; }

    [MaxLength(32)]
    public string? Password { get; init; }
}
