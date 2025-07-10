using HexMaster.MemeIt.Games.Features.LeaveGame;

namespace HexMaster.MemeIt.Games.Tests.Features.LeaveGame;

public class LeaveGameCommandTests
{
    [Fact]
    public void LeaveGameCommand_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var playerId = "player123";
        var gameCode = "ABC123";

        // Act
        var command = new LeaveGameCommand(playerId, gameCode);

        // Assert
        Assert.Equal(playerId, command.PlayerId);
        Assert.Equal(gameCode, command.GameCode);
        Assert.NotEqual(Guid.Empty, command.CommandId);
    }

    [Fact]
    public void LeaveGameCommand_CommandId_IsUniqueForEachInstance()
    {
        // Arrange & Act
        var command1 = new LeaveGameCommand("player1", "ABC123");
        var command2 = new LeaveGameCommand("player2", "ABC123");

        // Assert
        Assert.NotEqual(command1.CommandId, command2.CommandId);
    }

    [Theory]
    [InlineData("player1", "GAME01")]
    [InlineData("player2", "GAME02")]
    [InlineData("admin", "ABC123")]
    [InlineData("user-guid-123", "XYZ789")]
    public void LeaveGameCommand_AcceptsVariousInputs(string playerId, string gameCode)
    {
        // Act
        var command = new LeaveGameCommand(playerId, gameCode);

        // Assert
        Assert.Equal(playerId, command.PlayerId);
        Assert.Equal(gameCode, command.GameCode);
    }

    [Fact]
    public void LeaveGameCommand_IsRecord_SupportsEquality()
    {
        // Arrange
        var command1 = new LeaveGameCommand("player123", "ABC123");
        var command2 = new LeaveGameCommand("player123", "ABC123");

        // Act & Assert
        // Note: Records with generated CommandId will not be equal due to different GUIDs
        Assert.NotEqual(command1, command2); // Because CommandId is different
        Assert.Equal(command1.PlayerId, command2.PlayerId);
        Assert.Equal(command1.GameCode, command2.GameCode);
    }

    [Fact]
    public void LeaveGameCommand_WithEmptyStrings_AcceptsEmptyValues()
    {
        // Arrange & Act
        var command = new LeaveGameCommand("", "");

        // Assert
        Assert.Equal("", command.PlayerId);
        Assert.Equal("", command.GameCode);
    }

    [Fact]
    public void LeaveGameCommand_ToString_ContainsPlayerIdAndGameCode()
    {
        // Arrange
        var playerId = "player123";
        var gameCode = "ABC123";
        var command = new LeaveGameCommand(playerId, gameCode);

        // Act
        var toString = command.ToString();

        // Assert
        Assert.Contains(playerId, toString);
        Assert.Contains(gameCode, toString);
    }

    [Fact]
    public void LeaveGameCommand_WithNullValues_HandlesNulls()
    {
        // Arrange & Act
        var command = new LeaveGameCommand(null!, null!);

        // Assert
        Assert.Null(command.PlayerId);
        Assert.Null(command.GameCode);
    }
}
