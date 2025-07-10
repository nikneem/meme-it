using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Tests.ValueObjects;

public class GamePlayerStateTests
{
    [Fact]
    public void GamePlayerState_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var id = "player123";
        var name = "TestPlayer";

        // Act
        var gamePlayerState = new GamePlayerState
        {
            Id = id,
            Name = name
        };

        // Assert
        Assert.Equal(id, gamePlayerState.Id);
        Assert.Equal(name, gamePlayerState.Name);
    }

    [Theory]
    [InlineData("player1", "Alice")]
    [InlineData("player2", "Bob")]
    [InlineData("admin", "Administrator")]
    [InlineData("guid-123-456", "User With Spaces")]
    public void GamePlayerState_AcceptsVariousIdAndNameCombinations(string id, string name)
    {
        // Act
        var gamePlayerState = new GamePlayerState
        {
            Id = id,
            Name = name
        };

        // Assert
        Assert.Equal(id, gamePlayerState.Id);
        Assert.Equal(name, gamePlayerState.Name);
    }

    [Fact]
    public void GamePlayerState_WithEmptyId_StillWorks()
    {
        // Arrange
        var id = "";
        var name = "TestPlayer";

        // Act
        var gamePlayerState = new GamePlayerState
        {
            Id = id,
            Name = name
        };

        // Assert
        Assert.Equal(id, gamePlayerState.Id);
        Assert.Equal(name, gamePlayerState.Name);
    }

    [Fact]
    public void GamePlayerState_WithEmptyName_StillWorks()
    {
        // Arrange
        var id = "player123";
        var name = "";

        // Act
        var gamePlayerState = new GamePlayerState
        {
            Id = id,
            Name = name
        };

        // Assert
        Assert.Equal(id, gamePlayerState.Id);
        Assert.Equal(name, gamePlayerState.Name);
    }
}
