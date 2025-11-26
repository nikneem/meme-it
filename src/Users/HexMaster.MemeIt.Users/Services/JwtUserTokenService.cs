using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HexMaster.MemeIt.Users.Abstractions.Services;
using HexMaster.MemeIt.Users.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HexMaster.MemeIt.Users.Services;

/// <summary>
/// Issues and validates JWT tokens for the Users module.
/// </summary>
public sealed class JwtUserTokenService : IUserTokenService
{
    private readonly UsersJwtOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtUserTokenService(IOptions<UsersJwtOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        if (string.IsNullOrWhiteSpace(_options.SigningKey))
        {
            throw new InvalidOperationException("UsersJwt:SigningKey must be configured");
        }

        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _tokenHandler = new JwtSecurityTokenHandler();
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
    }

    public JwtToken CreateToken(Guid userId, string displayName)
    {
        var now = _timeProvider.GetUtcNow();
        var expires = now.AddMinutes(_options.ExpiryMinutes <= 0 ? 60 : _options.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: signingCredentials);

        var token = _tokenHandler.WriteToken(jwt);
        return new JwtToken(token, expires);
    }

    public UserIdentity ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token is required", nameof(token));
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = !string.IsNullOrWhiteSpace(_options.Issuer),
            ValidIssuer = _options.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(_options.Audience),
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
        var subject = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(subject, out var userId))
        {
            throw new SecurityTokenException("Token does not contain a valid user identifier");
        }

        var displayName = principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                         ?? principal.FindFirst(ClaimTypes.Name)?.Value;
        return new UserIdentity(userId, displayName);
    }
}
