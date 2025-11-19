namespace HexMaster.MemeIt.Users.Abstractions.Contracts;

/// <summary>
/// Response body returned after successfully creating/updating a user profile.
/// </summary>
/// <param name="UserId">Unique identifier for the player.</param>
/// <param name="DisplayName">The confirmed display name.</param>
/// <param name="Token">JWT bearer token for authenticated requests.</param>
/// <param name="ExpiresAt">Moment the token expires (UTC).</param>
public sealed record JoinUserResponse(Guid UserId, string DisplayName, string Token, DateTimeOffset ExpiresAt);
