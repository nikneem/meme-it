namespace HexMaster.MemeIt.Users.Abstractions.Application.Users;

/// <summary>
/// Result payload returned when a user joins the game service.
/// </summary>
/// <param name="UserId">The immutable identifier assigned to the user.</param>
/// <param name="DisplayName">The sanitized display name stored for the user.</param>
/// <param name="Token">Fresh JWT token for subsequent calls.</param>
/// <param name="ExpiresAt">Expiry moment for the new token.</param>
public sealed record JoinUserResult(Guid UserId, string DisplayName, string Token, DateTimeOffset ExpiresAt);
