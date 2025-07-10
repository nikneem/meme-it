using HexMaster.MemeIt.Games.Features.JoinGame;

namespace HexMaster.MemeIt.Games.Tests.Features.JoinGame;

public class JoinGameCommandTests
{
    [Fact]
    public void JoinGameCommand_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var playerName = "TestPlayer";
        var password = "secret";

        // Act
        var command = new JoinGameCommand
        {
            GameCode = gameCode,
            PlayerName = playerName,
            Password = password
        };

        // Assert
        Assert.Equal(gameCode, command.GameCode);
        Assert.Equal(playerName, command.PlayerName);
        Assert.Equal(password, command.Password);
        Assert.NotEqual(Guid.Empty, command.CommandId);
    }

    [Fact]
    public void JoinGameCommand_WithoutPassword_AllowsNullPassword()
    {
        // Arrange
        var gameCode = "ABC123";
        var playerName = "TestPlayer";

        // Act
        var command = new JoinGameCommand
        {
            GameCode = gameCode,
            PlayerName = playerName,
            Password = null
        };

        // Assert
        Assert.Equal(gameCode, command.GameCode);
        Assert.Equal(playerName, command.PlayerName);
        Assert.Null(command.Password);
    }

    [Fact]
    public void JoinGameCommand_CommandId_IsUniqueForEachInstance()
    {
        // Arrange & Act
        var command1 = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "Player1"
        };
        var command2 = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "Player2"
        };

        // Assert
        Assert.NotEqual(command1.CommandId, command2.CommandId);
    }

    [Theory]
    [InlineData("GAME01", "Alice", "pass123")]
    [InlineData("GAME02", "Bob", null)]
    [InlineData("GAME03", "Charlie", "")]
    [InlineData("GAME04", "David", "very-long-password-123")]
    public void JoinGameCommand_AcceptsVariousInputs(string gameCode, string playerName, string? password)
    {
        // Act
        var command = new JoinGameCommand
        {
            GameCode = gameCode,
            PlayerName = playerName,
            Password = password
        };

        // Assert
        Assert.Equal(gameCode, command.GameCode);
        Assert.Equal(playerName, command.PlayerName);
        Assert.Equal(password, command.Password);
    }

    [Fact]
    public void JoinGameCommand_IsRecord_SupportsEquality()
    {
        // Arrange
        var command1 = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "TestPlayer",
            Password = "secret"
        };
        var command2 = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "TestPlayer",
            Password = "secret"
        };

        // Act & Assert
        // Note: Records with generated CommandId will not be equal due to different GUIDs
        Assert.NotEqual(command1, command2); // Because CommandId is different
        Assert.Equal(command1.GameCode, command2.GameCode);
        Assert.Equal(command1.PlayerName, command2.PlayerName);
        Assert.Equal(command1.Password, command2.Password);
    }

    [Fact]
    public void JoinGameCommand_WithEmptyStrings_AcceptsEmptyValues()
    {
        // Arrange & Act
        var command = new JoinGameCommand
        {
            GameCode = "",
            PlayerName = "",
            Password = ""
        };

        // Assert
        Assert.Equal("", command.GameCode);
        Assert.Equal("", command.PlayerName);
        Assert.Equal("", command.Password);
    }
}
