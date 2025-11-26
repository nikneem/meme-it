using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HexMaster.MemeIt.Games.Api.Infrastructure.Identity;

/// <summary>
/// Validates bearer tokens issued by the Users service and extracts player identity details.
/// </summary>
public sealed class JwtPlayerIdentityProvider : IPlayerIdentityProvider
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly TokenValidationParameters _validationParameters;

    public JwtPlayerIdentityProvider(IOptions<UsersJwtOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var jwtOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("UsersJwt:SigningKey must be configured for the Games API");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtOptions.Issuer),
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtOptions.Audience),
            ValidAudience = jwtOptions.Audience,
            ClockSkew = TimeSpan.FromSeconds(Math.Max(0, jwtOptions.ClockSkewSeconds))
        };
    }

    public PlayerIdentity GetIdentity(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = ExtractBearerToken(request.Headers);
        if (token is null)
        {
            throw new UnauthorizedAccessException("Authorization header with a bearer token is required.");
        }

        try
        {
            var principal = _tokenHandler.ValidateToken(token, _validationParameters, out _);
            var userId = ResolveUserId(principal);
            var displayName = ResolveDisplayName(principal);
            return new PlayerIdentity(userId, displayName);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            throw new UnauthorizedAccessException("Authorization token is invalid or expired.", ex);
        }
    }

    private static Guid ResolveUserId(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Authorization token does not contain a valid user id.");
    }

    private static string ResolveDisplayName(ClaimsPrincipal principal)
    {
        var displayName = principal.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                         ?? principal.FindFirstValue(ClaimTypes.Name);

        return !string.IsNullOrWhiteSpace(displayName)
            ? displayName
            : throw new UnauthorizedAccessException("Authorization token does not contain a player name.");
    }

    private static string? ExtractBearerToken(IHeaderDictionary headers)
    {
        if (!headers.TryGetValue("Authorization", out var headerValues))
        {
            return null;
        }

        var headerValue = headerValues.ToString();
        const string bearerPrefix = "Bearer ";
        if (!headerValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = headerValue[bearerPrefix.Length..].Trim();
        return string.IsNullOrEmpty(token) ? null : token;
    }
}
