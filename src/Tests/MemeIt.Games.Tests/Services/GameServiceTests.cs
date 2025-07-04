using MemeIt.Core.Models;
using MemeIt.Games.Abstractions.Grains;
using MemeIt.Games.Abstractions.Models;
using MemeIt.Games.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Orleans;

namespace MemeIt.Games.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<ILogger<GameService>> _mockLogger;
    private readonly Mock<IGameGrain> _mockGameGrain;
    private readonly Mock<IGameRegistryGrain> _mockGameRegistryGrain;
    private readonly GameService _gameService;

    public GameServiceTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockLogger = new Mock<ILogger<GameService>>();
        _mockGameGrain = new Mock<IGameGrain>();
        _mockGameRegistryGrain = new Mock<IGameRegistryGrain>();
        
        _gameService = new GameService(_mockGrainFactory.Object, _mockLogger.Object);
    }

    private void SetupMockGrains()
    {
        _mockGrainFactory.Setup(x => x.GetGrain<IGameGrain>(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns(_mockGameGrain.Object);
        
        // Use the same pattern as the working IGameGrain setup
        _mockGrainFactory.Setup(x => x.GetGrain<IGameRegistryGrain>(It.IsAny<int>(), It.IsAny<string>()))
                        .Returns(_mockGameRegistryGrain.Object);
        
        // Also handle the case where key string might be null for registryGrain  
        _mockGrainFactory.Setup(x => x.GetGrain<IGameRegistryGrain>(It.IsAny<int>(), null))
                        .Returns(_mockGameRegistryGrain.Object);
    }

    [Fact]
    public async Task CreateGameAsync_ShouldReturnGameIdAndCode()
    {
        // Arrange
        SetupMockGrains();
        var gameState = new GameState 
        { 
            Id = "test-game-id", 
            GameCode = "ABC123",
            Name = "Test Game",
            GameMasterId = "player1",
            Status = GameStatus.Lobby
        };
        
        _mockGameGrain.Setup(x => x.JoinGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(true);
        _mockGameGrain.Setup(x => x.GetGameStateAsync())
                     .ReturnsAsync(gameState);

        // Act
        var (gameId, gameCode) = await _gameService.CreateGameAsync("player1", "Alice", "Test Game");

        // Assert
        Assert.NotNull(gameId);
        Assert.NotNull(gameCode);
        Assert.Equal("ABC123", gameCode);
        
        // Verify grain interactions
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>(It.IsAny<string>(), null), Times.Once);
        _mockGameGrain.Verify(x => x.JoinGameAsync("player1", "Alice", null), Times.Once);
    }

    [Fact]
    public async Task CreateGameAsync_WithNullPlayerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync(null!, "Alice", "Test Game"));
        Assert.Contains("playerId", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithEmptyPlayerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("", "Alice", "Test Game"));
        Assert.Contains("playerId", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithWhitespacePlayerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("   ", "Alice", "Test Game"));
        Assert.Contains("playerId", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithNullPlayerName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("player1", null!, "Test Game"));
        Assert.Contains("playerName", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithEmptyPlayerName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("player1", "", "Test Game"));
        Assert.Contains("playerName", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithWhitespacePlayerName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("player1", "   ", "Test Game"));
        Assert.Contains("playerName", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithNullGameName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("player1", "Alice", null!));
        Assert.Contains("gameName", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithEmptyGameName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("player1", "Alice", ""));
        Assert.Contains("gameName", exception.Message);
    }

    [Fact]
    public async Task CreateGameAsync_WithWhitespaceGameName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.CreateGameAsync("player1", "Alice", "   "));
        Assert.Contains("gameName", exception.Message);
    }

    [Fact]
    public async Task GetGameAsync_WithValidId_ShouldReturnGameState()
    {
        // Arrange
        SetupMockGrains();
        var gameState = new GameState 
        { 
            Id = "game1", 
            GameCode = "ABC123",
            Name = "Test Game" 
        };
        
        _mockGameGrain.Setup(x => x.GetGameStateAsync())
                     .ReturnsAsync(gameState);

        // Act
        var result = await _gameService.GetGameAsync("game1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("game1", result.Id);
        Assert.Equal("ABC123", result.GameCode);
        Assert.Equal("Test Game", result.Name);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
    }

    [Fact]
    public async Task GetGameAsync_WithNullId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.GetGameAsync(null!));
        Assert.Contains("gameId", exception.Message);
    }

    [Fact]
    public async Task GetGameAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.GetGameAsync(""));
        Assert.Contains("gameId", exception.Message);
    }

    [Fact]
    public async Task GetGameAsync_WithWhitespaceId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.GetGameAsync("   "));
        Assert.Contains("gameId", exception.Message);
    }



    [Fact]
    public async Task GetGameByCodeAsync_WithInvalidCode_ShouldReturnNull()
    {
        // Arrange
        SetupMockGrains();
        // Invalid code format (not 6 characters) should not reach the registry grain

        // Act
        var result = await _gameService.GetGameByCodeAsync("INVALID");

        // Assert
        Assert.Null(result);
        
        // Verify that the registry grain is never called for invalid format codes
        _mockGameRegistryGrain.Verify(x => x.GetGameIdByCodeAsync(It.IsAny<string>()), Times.Never);
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>(It.IsAny<string>(), null), Times.Never);
    }

    [Fact]
    public async Task GetGameByCodeAsync_WithNullCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.GetGameByCodeAsync(null!));
        Assert.Contains("gameCode", exception.Message);
    }

    [Fact]
    public async Task GetGameByCodeAsync_WithEmptyCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.GetGameByCodeAsync(""));
        Assert.Contains("gameCode", exception.Message);
    }

    [Fact]
    public async Task GetGameByCodeAsync_WithWhitespaceCode_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.GetGameByCodeAsync("   "));
        Assert.Contains("gameCode", exception.Message);
    }

    [Fact]
    public async Task JoinGameAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.JoinGameAsync("player1", "Alice", null))
                     .ReturnsAsync(true);

        // Act
        var result = await _gameService.JoinGameAsync("game1", "player1", "Alice");

        // Assert
        Assert.True(result);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.JoinGameAsync("player1", "Alice", null), Times.Once);
    }

    [Fact]
    public async Task JoinGameAsync_WithNullGameId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.JoinGameAsync(null!, "player1", "Alice"));
        Assert.Contains("gameId", exception.Message);
    }

    [Fact]
    public async Task JoinGameAsync_WithNullPlayerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.JoinGameAsync("game1", null!, "Alice"));
        Assert.Contains("playerId", exception.Message);
    }

    [Fact]
    public async Task JoinGameAsync_WithNullPlayerName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.JoinGameAsync("game1", "player1", null!));
        Assert.Contains("playerName", exception.Message);
    }


    [Fact]
    public async Task JoinGameByCodeAsync_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        SetupMockGrains();
        // Invalid code format (not 6 characters) should not reach the registry grain

        // Act
        var result = await _gameService.JoinGameByCodeAsync("INVALID", "player1", "Alice");

        // Assert
        Assert.False(result);
        
        // Verify that the registry grain is never called for invalid format codes
        _mockGameRegistryGrain.Verify(x => x.GetGameIdByCodeAsync(It.IsAny<string>()), Times.Never);
        _mockGameGrain.Verify(x => x.JoinGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task StartGameAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.StartGameAsync("player1"))
                     .ReturnsAsync(true);

        // Act
        var result = await _gameService.StartGameAsync("game1", "player1");

        // Assert
        Assert.True(result);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.StartGameAsync("player1"), Times.Once);
    }

    [Fact]
    public async Task SubmitMemeTextAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        SetupMockGrains();
        var textEntries = new Dictionary<string, string> { { "top", "Test text" } };
        _mockGameGrain.Setup(x => x.SubmitMemeTextAsync("player1", textEntries))
                     .ReturnsAsync(true);

        // Act
        var result = await _gameService.SubmitMemeTextAsync("game1", "player1", textEntries);

        // Assert
        Assert.True(result);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.SubmitMemeTextAsync("player1", textEntries), Times.Once);
    }

    [Fact]
    public async Task SubmitMemeTextAsync_WithNullTextEntries_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitMemeTextAsync("game1", "player1", null!));
        Assert.Contains("textEntries", exception.Message);
    }

    [Fact]
    public async Task SubmitMemeTextAsync_WithNullGameId_ShouldThrowArgumentException()
    {
        // Arrange
        var textEntries = new Dictionary<string, string> { { "top", "Test" } };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitMemeTextAsync(null!, "player1", textEntries));
        Assert.Contains("gameId", exception.Message);
    }

    [Fact]
    public async Task SubmitMemeTextAsync_WithNullPlayerId_ShouldThrowArgumentException()
    {
        // Arrange
        var textEntries = new Dictionary<string, string> { { "top", "Test" } };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitMemeTextAsync("game1", null!, textEntries));
        Assert.Contains("playerId", exception.Message);
    }

    [Fact]
    public async Task SubmitScoreAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.SubmitScoreAsync("player1", "player2", 5))
                     .ReturnsAsync(true);

        // Act
        var result = await _gameService.SubmitScoreAsync("game1", "player1", "player2", 5);

        // Assert
        Assert.True(result);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.SubmitScoreAsync("player1", "player2", 5), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    [InlineData(10)]
    public async Task SubmitScoreAsync_WithInvalidScore_ShouldThrowArgumentException(int invalidScore)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitScoreAsync("game1", "player1", "player2", invalidScore));
        Assert.Contains("score", exception.Message);
    }

    [Fact]
    public async Task SubmitScoreAsync_WithNullGameId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitScoreAsync(null!, "player1", "player2", 5));
        Assert.Contains("gameId", exception.Message);
    }

    [Fact]
    public async Task SubmitScoreAsync_WithNullPlayerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitScoreAsync("game1", null!, "player2", 5));
        Assert.Contains("playerId", exception.Message);
    }

    [Fact]
    public async Task SubmitScoreAsync_WithNullTargetPlayerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _gameService.SubmitScoreAsync("game1", "player1", null!, 5));
        Assert.Contains("targetPlayerId", exception.Message);
    }

    [Fact]
    public async Task GetScoresAsync_WithValidGameId_ShouldReturnScores()
    {
        // Arrange
        SetupMockGrains();
        var scores = new Dictionary<string, int> { { "player1", 10 }, { "player2", 15 } };
        _mockGameGrain.Setup(x => x.GetScoresAsync())
                     .ReturnsAsync(scores);

        // Act
        var result = await _gameService.GetScoresAsync("game1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(10, result["player1"]);
        Assert.Equal(15, result["player2"]);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.GetScoresAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCurrentRoundAsync_WithValidGameId_ShouldReturnRoundState()
    {
        // Arrange
        SetupMockGrains();
        var roundState = new RoundState 
        { 
            RoundNumber = 1, 
            Status = RoundStatus.TextEntry 
        };
        _mockGameGrain.Setup(x => x.GetCurrentRoundAsync())
                     .ReturnsAsync(roundState);

        // Act
        var result = await _gameService.GetCurrentRoundAsync("game1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.RoundNumber);
        Assert.Equal(RoundStatus.TextEntry, result.Status);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.GetCurrentRoundAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateGameAsync_WithOptions_ShouldUpdateOptionsAndReturnCode()
    {
        // Arrange
        SetupMockGrains();
        var gameState = new GameState 
        { 
            Id = "test-game-id", 
            GameCode = "XYZ789",
            Name = "Test Game with Options",
            GameMasterId = "player1",
            Status = GameStatus.Lobby
        };
        
        var options = new GameOptions 
        { 
            MaxPlayers = 6, 
            MinPlayers = 3,
            IsPrivate = true,
            Password = "secret123"
        };
        
        _mockGameGrain.Setup(x => x.JoinGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(true);
        _mockGameGrain.Setup(x => x.UpdateGameOptionsAsync(It.IsAny<string>(), It.IsAny<GameOptions>()))
                     .ReturnsAsync(true);
        _mockGameGrain.Setup(x => x.GetGameStateAsync())
                     .ReturnsAsync(gameState);

        // Act
        var (gameId, gameCode) = await _gameService.CreateGameAsync("player1", "Alice", "Test Game with Options", options);

        // Assert
        Assert.NotNull(gameId);
        Assert.Equal("XYZ789", gameCode);
        
        // Verify option update was called
        _mockGameGrain.Verify(x => x.UpdateGameOptionsAsync("player1", options), Times.Once);
    }

    [Fact]
    public async Task GetAvailableGamesAsync_ShouldReturnEmptyList()
    {
        // Act
        var result = await _gameService.GetAvailableGamesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetGameAsync_WithException_ShouldReturnNull()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.GetGameStateAsync())
                     .ThrowsAsync(new Exception("Grain not found"));

        // Act
        var result = await _gameService.GetGameAsync("invalid-game");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task JoinGameAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.JoinGameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.JoinGameAsync("game1", "player1", "Alice");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LeaveGameAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.LeaveGameAsync("player1"))
                     .ReturnsAsync(true);

        // Act
        var result = await _gameService.LeaveGameAsync("game1", "player1");

        // Assert
        Assert.True(result);
        
        _mockGrainFactory.Verify(x => x.GetGrain<IGameGrain>("game1", null), Times.Once);
        _mockGameGrain.Verify(x => x.LeaveGameAsync("player1"), Times.Once);
    }

    [Fact]
    public async Task LeaveGameAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.LeaveGameAsync(It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.LeaveGameAsync("game1", "player1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task StartGameAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.StartGameAsync(It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.StartGameAsync("game1", "player1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SubmitMemeTextAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        SetupMockGrains();
        var textEntries = new Dictionary<string, string> { { "top", "Test text" } };
        _mockGameGrain.Setup(x => x.SubmitMemeTextAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.SubmitMemeTextAsync("game1", "player1", textEntries);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SubmitScoreAsync_WithException_ShouldReturnFalse()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.SubmitScoreAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.SubmitScoreAsync("game1", "player1", "player2", 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetScoresAsync_WithException_ShouldReturnEmptyDictionary()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.GetScoresAsync())
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.GetScoresAsync("game1");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCurrentRoundAsync_WithException_ShouldReturnNull()
    {
        // Arrange
        SetupMockGrains();
        _mockGameGrain.Setup(x => x.GetCurrentRoundAsync())
                     .ThrowsAsync(new Exception("Network error"));

        // Act
        var result = await _gameService.GetCurrentRoundAsync("game1");

        // Assert
        Assert.Null(result);
    }
}
