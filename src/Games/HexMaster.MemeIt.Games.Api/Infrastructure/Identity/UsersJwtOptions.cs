namespace HexMaster.MemeIt.Games.Api.Infrastructure.Identity;

/// <summary>
/// Configuration describing how to validate JWT tokens issued by the Users service.
/// </summary>
public sealed class UsersJwtOptions
{
    public const string SectionName = "UsersJwt";

    /// <summary>
    /// Shared symmetric key used to validate token signatures.
    /// </summary>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Expected issuer value. Optional.
    /// </summary>
    public string? Issuer { get; init; }
        = "HexMaster.MemeIt.Users";

    /// <summary>
    /// Expected audience value. Optional.
    /// </summary>
    public string? Audience { get; init; }
        = "HexMaster.MemeIt.Games";

    /// <summary>
    /// Allowed clock skew (seconds) when validating token lifetime.
    /// </summary>
    public int ClockSkewSeconds { get; init; } = 60;
}
