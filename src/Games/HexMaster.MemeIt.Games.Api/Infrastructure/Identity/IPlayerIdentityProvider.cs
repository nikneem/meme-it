namespace HexMaster.MemeIt.Games.Api.Infrastructure.Identity;

/// <summary>
/// Resolves player identity details from incoming HTTP requests.
/// </summary>
public interface IPlayerIdentityProvider
{
    /// <summary>
    /// Extracts and validates the authenticated player identity from the request.
    /// </summary>
    /// <param name="request">Incoming HTTP request.</param>
    /// <returns>The authenticated player identity.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when the authorization header is missing or invalid.</exception>
    PlayerIdentity GetIdentity(HttpRequest request);
}
