namespace HexMaster.MemeIt.Users.Options;

/// <summary>
/// Configuration settings required to mint and validate JWT tokens in the Users service.
/// </summary>
public sealed class UsersJwtOptions
{
    public const string SectionName = "UsersJwt";

    /// <summary>
    /// Symmetric signing key used for HMAC signatures.
    /// </summary>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Optional issuer value appended to generated tokens.
    /// </summary>
    public string? Issuer { get; init; }
        = "HexMaster.MemeIt.Users";

    /// <summary>
    /// Optional audience restriction for generated tokens.
    /// </summary>
    public string? Audience { get; init; }
        = "HexMaster.MemeIt.Clients";

    /// <summary>
    /// Token lifetime in minutes. Defaults to one day.
    /// </summary>
    public int ExpiryMinutes { get; init; } = 60 * 24;
}
