namespace HexMaster.MemeIt.Users.Abstractions.Services;

/// <summary>
/// Provides JWT token creation and validation services for the Users module.
/// </summary>
public interface IUserTokenService
{
    /// <summary>
    /// Creates a signed JWT token for the provided identity.
    /// </summary>
    /// <param name="userId">Identifier to embed.</param>
    /// <param name="displayName">Display name claim to embed.</param>
    /// <returns>Token metadata.</returns>
    JwtToken CreateToken(Guid userId, string displayName);

    /// <summary>
    /// Validates an incoming token and extracts the embedded identity.
    /// </summary>
    /// <param name="token">Bearer token supplied by the caller.</param>
    /// <returns>Extracted identity information.</returns>
    UserIdentity ValidateToken(string token);
}

/// <summary>
/// Lightweight representation of a persisted JWT.
/// </summary>
/// <param name="Token">The serialized JWT string.</param>
/// <param name="ExpiresAt">Moment the token expires.</param>
public sealed record JwtToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// Identity information derived from an incoming JWT.
/// </summary>
/// <param name="UserId">Identifier embedded in the token.</param>
/// <param name="DisplayName">Optional display name stored in the token.</param>
public sealed record UserIdentity(Guid UserId, string? DisplayName);
