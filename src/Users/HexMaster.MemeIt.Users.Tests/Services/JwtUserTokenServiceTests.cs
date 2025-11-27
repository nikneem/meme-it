using HexMaster.MemeIt.Users.Options;
using HexMaster.MemeIt.Users.Services;
using Microsoft.IdentityModel.Tokens;

namespace HexMaster.MemeIt.Users.Tests.Services;

public sealed class JwtUserTokenServiceTests
{
    [Fact]
    public void CreateToken_Returns_Signed_Token_With_Configured_Expiry()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var timeProvider = new FixedTimeProvider(now);
        var options = Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
        {
            SigningKey = new string('a', 64),
            Issuer = "issuer",
            Audience = "audience",
            ExpiryMinutes = 60
        });

        var service = new JwtUserTokenService(options, timeProvider);

        // Act
        var token = service.CreateToken(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Alice");

        // Assert
        Assert.Equal(now.AddMinutes(60), token.ExpiresAt);
        var identity = service.ValidateToken(token.Token);
        Assert.Equal(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), identity.UserId);
        Assert.Equal("Alice", identity.DisplayName);
    }

    [Fact]
    public void ValidateToken_Throws_For_Tampered_Token()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var service = new JwtUserTokenService(
            Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
            {
                SigningKey = new string('b', 64),
                Issuer = "issuer",
                Audience = "audience"
            }),
            timeProvider);

        var token = service.CreateToken(Guid.NewGuid(), "Bob").Token;
        var tamperedToken = token + "tampered";

        // Act & Assert
        Assert.ThrowsAny<SecurityTokenException>(() => service.ValidateToken(tamperedToken));
    }

    [Fact]
    public void ValidateToken_Throws_For_Empty_Token()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var service = new JwtUserTokenService(
            Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
            {
                SigningKey = new string('b', 64),
                Issuer = "issuer",
                Audience = "audience"
            }),
            timeProvider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.ValidateToken(""));
    }

    [Fact]
    public void ValidateToken_Throws_For_Null_Token()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var service = new JwtUserTokenService(
            Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
            {
                SigningKey = new string('b', 64),
                Issuer = "issuer",
                Audience = "audience"
            }),
            timeProvider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.ValidateToken(null!));
    }

    [Fact]
    public void Constructor_Throws_When_Options_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtUserTokenService(null!, TimeProvider.System));
    }

    [Fact]
    public void Constructor_Throws_When_TimeProvider_Is_Null()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
        {
            SigningKey = new string('a', 64)
        });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JwtUserTokenService(options, null!));
    }

    [Fact]
    public void Constructor_Throws_When_SigningKey_Is_Empty()
    {
        // Arrange
        var options = Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
        {
            SigningKey = ""
        });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new JwtUserTokenService(options, TimeProvider.System));
    }

    [Fact]
    public void CreateToken_Uses_Default_Expiry_When_ExpiryMinutes_Is_Zero()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var timeProvider = new FixedTimeProvider(now);
        var options = Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
        {
            SigningKey = new string('a', 64),
            Issuer = "issuer",
            Audience = "audience",
            ExpiryMinutes = 0
        });

        var service = new JwtUserTokenService(options, timeProvider);

        // Act
        var token = service.CreateToken(Guid.NewGuid(), "Alice");

        // Assert
        Assert.Equal(now.AddMinutes(60), token.ExpiresAt);
    }

    [Fact]
    public void CreateToken_Includes_UserId_In_Claims()
    {
        // Arrange
        var userId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var options = Microsoft.Extensions.Options.Options.Create(new UsersJwtOptions
        {
            SigningKey = new string('a', 64),
            Issuer = "issuer",
            Audience = "audience"
        });

        var service = new JwtUserTokenService(options, timeProvider);

        // Act
        var token = service.CreateToken(userId, "TestUser");
        var identity = service.ValidateToken(token.Token);

        // Assert
        Assert.Equal(userId, identity.UserId);
        Assert.Equal("TestUser", identity.DisplayName);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
