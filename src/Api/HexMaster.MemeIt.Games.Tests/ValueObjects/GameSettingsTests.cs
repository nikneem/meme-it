using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Tests.ValueObjects;

public class GameSettingsTests
{
    [Fact]
    public void GameSettings_DefaultConstructor_SetsDefaultValues()
    {
        // Arrange & Act
        var gameSettings = new GameSettings();

        // Assert
        Assert.Equal(10, gameSettings.MaxPlayers);
        Assert.Equal(5, gameSettings.NumberOfRounds);
        Assert.Equal("All", gameSettings.Category);
    }

    [Fact]
    public void GameSettings_SetMaxPlayers_UpdatesValue()
    {
        // Arrange
        var gameSettings = new GameSettings();

        // Act
        gameSettings.MaxPlayers = 8;

        // Assert
        Assert.Equal(8, gameSettings.MaxPlayers);
    }

    [Fact]
    public void GameSettings_SetNumberOfRounds_UpdatesValue()
    {
        // Arrange
        var gameSettings = new GameSettings();

        // Act
        gameSettings.NumberOfRounds = 3;

        // Assert
        Assert.Equal(3, gameSettings.NumberOfRounds);
    }

    [Fact]
    public void GameSettings_SetCategory_UpdatesValue()
    {
        // Arrange
        var gameSettings = new GameSettings();

        // Act
        gameSettings.Category = "Movies";

        // Assert
        Assert.Equal("Movies", gameSettings.Category);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void GameSettings_MaxPlayers_AcceptsValidValues(int maxPlayers)
    {
        // Arrange
        var gameSettings = new GameSettings();

        // Act
        gameSettings.MaxPlayers = maxPlayers;

        // Assert
        Assert.Equal(maxPlayers, gameSettings.MaxPlayers);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void GameSettings_NumberOfRounds_AcceptsValidValues(int numberOfRounds)
    {
        // Arrange
        var gameSettings = new GameSettings();

        // Act
        gameSettings.NumberOfRounds = numberOfRounds;

        // Assert
        Assert.Equal(numberOfRounds, gameSettings.NumberOfRounds);
    }

    [Theory]
    [InlineData("All")]
    [InlineData("Movies")]
    [InlineData("Sports")]
    [InlineData("Technology")]
    public void GameSettings_Category_AcceptsValidValues(string category)
    {
        // Arrange
        var gameSettings = new GameSettings();

        // Act
        gameSettings.Category = category;

        // Assert
        Assert.Equal(category, gameSettings.Category);
    }
}
