using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Tests.ValueObjects;

public class GameStateTests
{
    [Fact]
    public void GameState_Constructor_SetsRequiredProperties()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)> { ("player1", "Alice") };

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players
        };

        // Assert
        Assert.Equal(gameCode, gameState.GameCode);
        Assert.Equal(players, gameState.Players);
        Assert.Equal(GameStatus.Uninitialized.Id, gameState.Status);
        Assert.NotNull(gameState.Settings);
        Assert.Null(gameState.Password);
        Assert.Null(gameState.LeaderId);
    }

    [Fact]
    public void GameState_WithPassword_SetsPasswordCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)>();
        var password = "secret123";

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players,
            Password = password
        };

        // Assert
        Assert.Equal(password, gameState.Password);
    }

    [Fact]
    public void GameState_WithLeaderId_SetsLeaderIdCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)>();
        var leaderId = "player1";

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players,
            LeaderId = leaderId
        };

        // Assert
        Assert.Equal(leaderId, gameState.LeaderId);
    }

    [Fact]
    public void GameState_WithCustomStatus_SetsStatusCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)>();
        var status = GameStatus.Waiting.Id;

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = status,
            Players = players
        };

        // Assert
        Assert.Equal(status, gameState.Status);
    }

    [Fact]
    public void GameState_WithCustomSettings_SetsSettingsCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)>();
        var settings = new GameSettings
        {
            MaxPlayers = 8,
            NumberOfRounds = 3,
            Category = "Movies"
        };

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players,
            Settings = settings
        };

        // Assert
        Assert.Equal(settings, gameState.Settings);
        Assert.Equal(8, gameState.Settings.MaxPlayers);
        Assert.Equal(3, gameState.Settings.NumberOfRounds);
        Assert.Equal("Movies", gameState.Settings.Category);
    }

    [Fact]
    public void GameState_WithMultiplePlayers_HandlesPlayersList()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)>
        {
            ("player1", "Alice"),
            ("player2", "Bob"),
            ("player3", "Charlie")
        };

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players
        };

        // Assert
        Assert.Equal(3, gameState.Players.Count);
        Assert.Contains(("player1", "Alice"), gameState.Players);
        Assert.Contains(("player2", "Bob"), gameState.Players);
        Assert.Contains(("player3", "Charlie"), gameState.Players);
    }

    [Fact]
    public void GameState_WithEmptyPlayersList_AcceptsEmptyList()
    {
        // Arrange
        var gameCode = "ABC123";
        var players = new List<(string Id, string Name)>();

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players
        };

        // Assert
        Assert.Empty(gameState.Players);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("XYZ999")]
    [InlineData("TEST01")]
    public void GameState_WithDifferentGameCodes_AcceptsVariousCodes(string gameCode)
    {
        // Arrange
        var players = new List<(string Id, string Name)>();

        // Act
        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Uninitialized.Id,
            Players = players
        };

        // Assert
        Assert.Equal(gameCode, gameState.GameCode);
    }
}
