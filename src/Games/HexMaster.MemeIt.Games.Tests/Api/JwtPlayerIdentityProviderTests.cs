using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HexMaster.MemeIt.Games.Api.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HexMaster.MemeIt.Games.Tests.Api;

public sealed class JwtPlayerIdentityProviderTests
{
    [Fact]
    public void GetIdentity_ReturnsIdentity_ForValidToken()
    {
        // Arrange
        var options = CreateOptions();
        var provider = new JwtPlayerIdentityProvider(options);
        var userId = Guid.NewGuid();
        var request = BuildRequest(options.Value, userId, "PlayerOne");

        // Act
        var identity = provider.GetIdentity(request);

        // Assert
        Assert.Equal(userId, identity.UserId);
        Assert.Equal("PlayerOne", identity.DisplayName);
    }

    [Fact]
    public void GetIdentity_Throws_WhenHeaderMissing()
    {
        var provider = new JwtPlayerIdentityProvider(CreateOptions());
        var request = new DefaultHttpContext().Request;

        Assert.Throws<UnauthorizedAccessException>(() => provider.GetIdentity(request));
    }

    [Fact]
    public void GetIdentity_Throws_WhenTokenInvalid()
    {
        var provider = new JwtPlayerIdentityProvider(CreateOptions());
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer invalid";

        Assert.Throws<UnauthorizedAccessException>(() => provider.GetIdentity(context.Request));
    }

    private static HttpRequest BuildRequest(UsersJwtOptions options, Guid userId, string displayName)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = handler.CreateJwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            subject: new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, displayName)
            }),
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Bearer {handler.WriteToken(token)}";
        return context.Request;
    }

    private static IOptions<UsersJwtOptions> CreateOptions()
        => Options.Create(new UsersJwtOptions
        {
            SigningKey = new string('a', 64),
            Issuer = "issuer",
            Audience = "audience",
            ClockSkewSeconds = 0
        });
}
