using System.ComponentModel.DataAnnotations;

namespace HexMaster.MemeIt.Users.Abstractions.Contracts;

/// <summary>
/// Request payload posted when a player wants to join the platform.
/// </summary>
public sealed record JoinUserRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(32)]
    public string DisplayName { get; init; } = string.Empty;
}
