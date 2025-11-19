namespace HexMaster.MemeIt.Games.Api.Infrastructure.Identity;

/// <summary>
/// Strongly typed identity information extracted from an authenticated player token.
/// </summary>
/// <param name="UserId">Unique identifier of the authenticated player.</param>
/// <param name="DisplayName">Display name embedded in the token.</param>
public sealed record PlayerIdentity(Guid UserId, string DisplayName);
