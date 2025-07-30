using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Tests.Features;

public class GameDetailsResponseTests
{
    [Fact]
    public void GameDetailsResponse_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var status = "Waiting";
        var players = new List<PlayerResponse> 
        { 
            new PlayerResponse("player1", "Alice", false), 
            new PlayerResponse("player2", "Bob", false) 
        };
        var playerId = "player1";
        var isPasswordProtected = true;
        var settings = new GameSettingsResponse(8, 3, "Movies");

        // Act
        var response = new GameDetailsResponse(gameCode, status, players, playerId, isPasswordProtected, settings);

        // Assert
        Assert.Equal(gameCode, response.GameCode);
        Assert.Equal(status, response.Status);
        Assert.Equal(players, response.Players);
        Assert.Equal(playerId, response.PlayerId);
        Assert.Equal(isPasswordProtected, response.IsPasswordProtected);
        Assert.Equal(settings, response.Settings);
    }

    [Fact]
    public void GameDetailsResponse_FromGameState_ConvertsCorrectly()
    {
        // Arrange
        var gameState = new GameState
        {
            GameCode = "TEST123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)>
            {
                ("player1", "Alice"),
                ("player2", "Bob"),
                ("player3", "Charlie")
            },
            Password = "secret",
            Settings = new GameSettings
            {
                MaxPlayers = 6,
                NumberOfRounds = 4,
                Category = "Sports"
            }
        };

        // Act
        var response = GameDetailsResponse.FromGameState(gameState, "player1");

        // Assert
        Assert.Equal("TEST123", response.GameCode);
        Assert.Equal(3, response.Players.Count);
        Assert.Contains(response.Players, p => p.Name == "Alice");
        Assert.Contains(response.Players, p => p.Name == "Bob");
        Assert.Contains(response.Players, p => p.Name == "Charlie");
        Assert.True(response.IsPasswordProtected);
        Assert.Equal(6, response.Settings.MaxPlayers);
        Assert.Equal(4, response.Settings.NumberOfRounds);
        Assert.Equal("Sports", response.Settings.Category);
    }

    [Fact]
    public void GameDetailsResponse_FromGameState_WithoutPassword_IsNotPasswordProtected()
    {
        // Arrange
        var gameState = new GameState
        {
            GameCode = "TEST123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)>(),
            Password = null
        };

        // Act
        var response = GameDetailsResponse.FromGameState(gameState, "player1");

        // Assert
        Assert.False(response.IsPasswordProtected);
    }

    [Fact]
    public void GameDetailsResponse_FromGameState_WithEmptyPassword_IsNotPasswordProtected()
    {
        // Arrange
        var gameState = new GameState
        {
            GameCode = "TEST123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)>(),
            Password = ""
        };

        // Act
        var response = GameDetailsResponse.FromGameState(gameState, "player1");

        // Assert
        Assert.False(response.IsPasswordProtected);
    }

    [Fact]
    public void GameDetailsResponse_FromGameState_WithWhitespacePassword_IsNotPasswordProtected()
    {
        // Arrange
        var gameState = new GameState
        {
            GameCode = "TEST123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)>(),
            Password = "   "
        };

        // Act
        var response = GameDetailsResponse.FromGameState(gameState, "player1");

        // Assert
        Assert.False(response.IsPasswordProtected);
    }

    [Fact]
    public void GameDetailsResponse_FromGameState_WithEmptyPlayersList_ReturnsEmptyPlayersList()
    {
        // Arrange
        var gameState = new GameState
        {
            GameCode = "TEST123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)>()
        };

        // Act
        var response = GameDetailsResponse.FromGameState(gameState, "player1");

        // Assert
        Assert.Empty(response.Players);
    }

    [Fact]
    public void GameDetailsResponse_FromGameState_ExtractsPlayerNamesOnly()
    {
        // Arrange
        var gameState = new GameState
        {
            GameCode = "TEST123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)>
            {
                ("id1", "Name1"),
                ("id2", "Name2"),
                ("id3", "Name3")
            }
        };

        // Act
        var response = GameDetailsResponse.FromGameState(gameState, "player1");

        // Assert
        Assert.Equal(3, response.Players.Count);
        Assert.Contains(response.Players, p => p.Name == "Name1");
        Assert.Contains(response.Players, p => p.Name == "Name2");
        Assert.Contains(response.Players, p => p.Name == "Name3");
        Assert.DoesNotContain(response.Players, p => p.Name == "id1");
        Assert.DoesNotContain(response.Players, p => p.Name == "id2");
        Assert.DoesNotContain(response.Players, p => p.Name == "id3");
    }
}

public class GameSettingsResponseTests
{
    [Fact]
    public void GameSettingsResponse_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var maxPlayers = 8;
        var numberOfRounds = 3;
        var category = "Movies";

        // Act
        var response = new GameSettingsResponse(maxPlayers, numberOfRounds, category);

        // Assert
        Assert.Equal(maxPlayers, response.MaxPlayers);
        Assert.Equal(numberOfRounds, response.NumberOfRounds);
        Assert.Equal(category, response.Category);
    }

    [Theory]
    [InlineData(1, 1, "All")]
    [InlineData(5, 3, "Sports")]
    [InlineData(10, 5, "Movies")]
    [InlineData(20, 10, "Technology")]
    public void GameSettingsResponse_AcceptsVariousValues(int maxPlayers, int numberOfRounds, string category)
    {
        // Act
        var response = new GameSettingsResponse(maxPlayers, numberOfRounds, category);

        // Assert
        Assert.Equal(maxPlayers, response.MaxPlayers);
        Assert.Equal(numberOfRounds, response.NumberOfRounds);
        Assert.Equal(category, response.Category);
    }
}
