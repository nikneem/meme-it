using System.ComponentModel.DataAnnotations;

namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request payload for joining an existing game.
/// </summary>
public sealed record JoinGameRequest
{
    [Required]
    [MaxLength(32)]
    public string PlayerName { get; init; } = string.Empty;

    [Required]
    [MaxLength(8)]
    public string GameCode { get; init; } = string.Empty;

    [MaxLength(32)]
    public string? Password { get; init; }
}
