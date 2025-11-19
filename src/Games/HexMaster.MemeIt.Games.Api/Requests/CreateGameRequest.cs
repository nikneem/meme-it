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
}
