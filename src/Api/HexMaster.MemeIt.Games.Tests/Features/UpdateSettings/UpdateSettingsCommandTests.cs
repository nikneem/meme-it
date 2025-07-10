using HexMaster.MemeIt.Games.Features.UpdateSettings;
using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Tests.Features.UpdateSettings;

public class UpdateSettingsCommandTests
{
    [Fact]
    public void UpdateSettingsCommand_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var playerId = "player123";
        var settings = new GameSettings { MaxPlayers = 8, NumberOfRounds = 3, Category = "Movies" };
        var gameCode = "ABC123";

        // Act
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);

        // Assert
        Assert.Equal(playerId, command.PlayerId);
        Assert.Equal(settings, command.Settings);
        Assert.Equal(gameCode, command.GameCode);
        Assert.NotEqual(Guid.Empty, command.CommandId);
    }

    [Fact]
    public void UpdateSettingsCommand_CommandId_IsUniqueForEachInstance()
    {
        // Arrange
        var settings = new GameSettings();

        // Act
        var command1 = new UpdateSettingsCommand("player1", settings, "ABC123");
        var command2 = new UpdateSettingsCommand("player2", settings, "ABC123");

        // Assert
        Assert.NotEqual(command1.CommandId, command2.CommandId);
    }

    [Theory]
    [InlineData("player1", "GAME01", 6, 4, "Sports")]
    [InlineData("player2", "GAME02", 10, 5, "Movies")]
    [InlineData("admin", "ABC123", 15, 7, "Technology")]
    public void UpdateSettingsCommand_AcceptsVariousInputs(string playerId, string gameCode, int maxPlayers, int numberOfRounds, string category)
    {
        // Arrange
        var settings = new GameSettings
        {
            MaxPlayers = maxPlayers,
            NumberOfRounds = numberOfRounds,
            Category = category
        };

        // Act
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);

        // Assert
        Assert.Equal(playerId, command.PlayerId);
        Assert.Equal(gameCode, command.GameCode);
        Assert.Equal(maxPlayers, command.Settings.MaxPlayers);
        Assert.Equal(numberOfRounds, command.Settings.NumberOfRounds);
        Assert.Equal(category, command.Settings.Category);
    }

    [Fact]
    public void UpdateSettingsCommand_IsRecord_SupportsEquality()
    {
        // Arrange
        var settings = new GameSettings { MaxPlayers = 8 };
        var command1 = new UpdateSettingsCommand("player123", settings, "ABC123");
        var command2 = new UpdateSettingsCommand("player123", settings, "ABC123");

        // Act & Assert
        // Note: Records with generated CommandId will not be equal due to different GUIDs
        Assert.NotEqual(command1, command2); // Because CommandId is different
        Assert.Equal(command1.PlayerId, command2.PlayerId);
        Assert.Equal(command1.GameCode, command2.GameCode);
        Assert.Equal(command1.Settings, command2.Settings);
    }

    [Fact]
    public void UpdateSettingsCommand_WithDefaultSettings_UsesDefaultValues()
    {
        // Arrange
        var playerId = "player123";
        var gameCode = "ABC123";
        var settings = new GameSettings(); // Default values

        // Act
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);

        // Assert
        Assert.Equal(10, command.Settings.MaxPlayers);
        Assert.Equal(5, command.Settings.NumberOfRounds);
        Assert.Equal("All", command.Settings.Category);
    }

    [Fact]
    public void UpdateSettingsCommand_ToString_ContainsKeyInformation()
    {
        // Arrange
        var playerId = "player123";
        var gameCode = "ABC123";
        var settings = new GameSettings { MaxPlayers = 8 };
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);

        // Act
        var toString = command.ToString();

        // Assert
        Assert.Contains(playerId, toString);
        Assert.Contains(gameCode, toString);
    }

    [Fact]
    public void UpdateSettingsCommand_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var playerId = "player123";
        var settings = new GameSettings { MaxPlayers = 8, NumberOfRounds = 3, Category = "Movies" };
        var gameCode = "ABC123";
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);

        // Act
        var (extractedPlayerId, extractedSettings, extractedGameCode) = command;

        // Assert
        Assert.Equal(playerId, extractedPlayerId);
        Assert.Equal(settings, extractedSettings);
        Assert.Equal(gameCode, extractedGameCode);
    }

    [Fact]
    public void UpdateSettingsCommand_WithCustomSettings_PreservesSettingsValues()
    {
        // Arrange
        var playerId = "player123";
        var gameCode = "ABC123";
        var settings = new GameSettings
        {
            MaxPlayers = 12,
            NumberOfRounds = 7,
            Category = "Custom Category"
        };

        // Act
        var command = new UpdateSettingsCommand(playerId, settings, gameCode);

        // Assert
        Assert.Equal(12, command.Settings.MaxPlayers);
        Assert.Equal(7, command.Settings.NumberOfRounds);
        Assert.Equal("Custom Category", command.Settings.Category);
    }

    [Fact]
    public void UpdateSettingsCommand_WithEmptyStrings_AcceptsEmptyValues()
    {
        // Arrange
        var settings = new GameSettings();

        // Act
        var command = new UpdateSettingsCommand("", settings, "");

        // Assert
        Assert.Equal("", command.PlayerId);
        Assert.Equal("", command.GameCode);
        Assert.NotNull(command.Settings);
    }
}
