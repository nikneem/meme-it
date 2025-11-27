using HexMaster.MemeIt.Users.Abstractions.Application.Users;
using HexMaster.MemeIt.Users.Abstractions.Services;
using HexMaster.MemeIt.Users.Application.Users.JoinUser;
using Moq;

namespace HexMaster.MemeIt.Users.Tests.Application;

public sealed class JoinUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Generates_New_User_When_Token_Missing()
    {
        // Arrange
        var tokenServiceMock = new Mock<IUserTokenService>();
        tokenServiceMock
            .Setup(service => service.CreateToken(It.IsAny<Guid>(), "PlayerOne"))
            .Returns<Guid, string>((userId, _) => new JwtToken($"token-{userId}", DateTimeOffset.UtcNow.AddDays(1)));

        var handler = new JoinUserCommandHandler(tokenServiceMock.Object);
        var command = new JoinUserCommand("PlayerOne", null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal("PlayerOne", result.DisplayName);
        Assert.NotEqual(Guid.Empty, result.UserId);
        Assert.StartsWith("token-", result.Token, StringComparison.Ordinal);

        tokenServiceMock.Verify(service => service.ValidateToken(It.IsAny<string>()), Times.Never);
        tokenServiceMock.Verify(service => service.CreateToken(It.Is<Guid>(id => id == result.UserId), "PlayerOne"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Reuses_UserId_When_Token_Present()
    {
        // Arrange
        var existingUserId = Guid.NewGuid();
        var tokenServiceMock = new Mock<IUserTokenService>();
        tokenServiceMock
            .Setup(service => service.ValidateToken("existing-token"))
            .Returns(new UserIdentity(existingUserId, "OldName"));
        tokenServiceMock
            .Setup(service => service.CreateToken(existingUserId, "NewName"))
            .Returns(new JwtToken("new-token", DateTimeOffset.UtcNow.AddDays(1)));

        var handler = new JoinUserCommandHandler(tokenServiceMock.Object);
        var command = new JoinUserCommand("NewName", "existing-token");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(existingUserId, result.UserId);
        Assert.Equal("NewName", result.DisplayName);
        Assert.Equal("new-token", result.Token);

        tokenServiceMock.Verify(service => service.ValidateToken("existing-token"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Throws_For_Invalid_DisplayName()
    {
        // Arrange
        var handler = new JoinUserCommandHandler(Mock.Of<IUserTokenService>());
        var command = new JoinUserCommand(" ", null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Command_Is_Null()
    {
        // Arrange
        var handler = new JoinUserCommandHandler(Mock.Of<IUserTokenService>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_DisplayName_Too_Short()
    {
        // Arrange
        var handler = new JoinUserCommandHandler(Mock.Of<IUserTokenService>());
        var command = new JoinUserCommand("A", null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_DisplayName_Too_Long()
    {
        // Arrange
        var handler = new JoinUserCommandHandler(Mock.Of<IUserTokenService>());
        var command = new JoinUserCommand(new string('A', 33), null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Trims_DisplayName()
    {
        // Arrange
        var tokenServiceMock = new Mock<IUserTokenService>();
        tokenServiceMock
            .Setup(service => service.CreateToken(It.IsAny<Guid>(), "Player"))
            .Returns<Guid, string>((userId, _) => new JwtToken($"token-{userId}", DateTimeOffset.UtcNow.AddDays(1)));

        var handler = new JoinUserCommandHandler(tokenServiceMock.Object);
        var command = new JoinUserCommand("  Player  ", null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal("Player", result.DisplayName);
        tokenServiceMock.Verify(service => service.CreateToken(It.IsAny<Guid>(), "Player"), Times.Once);
    }

    [Fact]
    public void Constructor_Throws_When_TokenService_Is_Null()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new JoinUserCommandHandler(null!));
    }
}
