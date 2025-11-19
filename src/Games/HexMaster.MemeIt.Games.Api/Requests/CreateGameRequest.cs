using System.ComponentModel.DataAnnotations;

namespace HexMaster.MemeIt.Games.Api.Requests;

/// <summary>
/// Request payload for creating a new game.
/// </summary>
public sealed record CreateGameRequest
{
    [Required]
    [MaxLength(32)]
    public string DisplayName { get; init; } = string.Empty;

    [MaxLength(32)]
    public string? Password { get; init; }
}
