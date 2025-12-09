using System.ComponentModel.DataAnnotations;

namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request payload for creating a new game.
/// </summary>
public sealed record CreateGameRequest
{
    [MaxLength(32)]
    public string? DisplayName { get; init; }

    [MaxLength(32)]
    public string? Password { get; init; }

    /// <summary>
    /// Optional game code of a previous game. When provided, invites players from the old game to join the new one.
    /// </summary>
    [MaxLength(8)]
    public string? PreviousGameCode { get; init; }
}
