using HexMaster.MemeIt.Games.Features.CreateGame;

namespace HexMaster.MemeIt.Games.Tests.Features.CreateGame;

public class CreateGameCommandTests
{
    [Fact]
    public void CreateGameCommand_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var playerName = "TestPlayer";
        var gameCode = "ABC123";
        var password = "secret";

        // Act
        var command = new CreateGameCommand
        {
            PlayerName = playerName,
            GameCode = gameCode,
            Password = password
        };

        // Assert
        Assert.Equal(playerName, command.PlayerName);
        Assert.Equal(gameCode, command.GameCode);
        Assert.Equal(password, command.Password);
        Assert.NotEqual(Guid.Empty, command.CommandId);
    }

    [Fact]
    public void CreateGameCommand_WithoutPassword_AllowsNullPassword()
    {
        // Arrange
        var playerName = "TestPlayer";
        var gameCode = "ABC123";

        // Act
        var command = new CreateGameCommand
        {
            PlayerName = playerName,
            GameCode = gameCode,
            Password = null
        };

        // Assert
        Assert.Equal(playerName, command.PlayerName);
        Assert.Equal(gameCode, command.GameCode);
        Assert.Null(command.Password);
    }

    [Fact]
    public void CreateGameCommand_CommandId_IsUniqueForEachInstance()
    {
        // Arrange & Act
        var command1 = new CreateGameCommand
        {
            PlayerName = "Player1",
            GameCode = "ABC123"
        };
        var command2 = new CreateGameCommand
        {
            PlayerName = "Player2",
            GameCode = "XYZ789"
        };

        // Assert
        Assert.NotEqual(command1.CommandId, command2.CommandId);
    }

    [Theory]
    [InlineData("Alice", "GAME01", "pass123")]
    [InlineData("Bob", "GAME02", null)]
    [InlineData("Charlie", "GAME03", "")]
    [InlineData("David", "GAME04", "very-long-password-123")]
    public void CreateGameCommand_AcceptsVariousInputs(string playerName, string gameCode, string? password)
    {
        // Act
        var command = new CreateGameCommand
        {
            PlayerName = playerName,
            GameCode = gameCode,
            Password = password
        };

        // Assert
        Assert.Equal(playerName, command.PlayerName);
        Assert.Equal(gameCode, command.GameCode);
        Assert.Equal(password, command.Password);
    }

    [Fact]
    public void CreateGameCommand_IsRecord_SupportsEquality()
    {
        // Arrange
        var command1 = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "ABC123",
            Password = "secret"
        };
        var command2 = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "ABC123",
            Password = "secret"
        };

        // Act & Assert
        // Note: Records with generated CommandId will not be equal due to different GUIDs
        Assert.NotEqual(command1, command2); // Because CommandId is different
        Assert.Equal(command1.PlayerName, command2.PlayerName);
        Assert.Equal(command1.GameCode, command2.GameCode);
        Assert.Equal(command1.Password, command2.Password);
    }
}
