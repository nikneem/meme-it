using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.JoinGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Moq;
using Orleans;

namespace HexMaster.MemeIt.Games.Tests.Features.JoinGame;

public class JoinGameCommandHandlerTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IGameGrain> _mockGameGrain;
    private readonly JoinGameCommandHandler _handler;

    public JoinGameCommandHandlerTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockGameGrain = new Mock<IGameGrain>();
        _handler = new JoinGameCommandHandler(_mockGrainFactory.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsGameDetailsResponse()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "TestPlayer",
            Password = "secret"
        };

        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)>
            {
                ("player1", "ExistingPlayer"),
                ("player2", "TestPlayer")
            },
            Status = GameStatus.Waiting.Id,
            Settings = new GameSettings()
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC123", result.GameCode);
        Assert.Equal(2, result.Players.Count);
        Assert.Contains(result.Players, p => p.Name == "ExistingPlayer");
        Assert.Contains(result.Players, p => p.Name == "TestPlayer");
    }

    [Fact]
    public async Task HandleAsync_CallsGetGrainWithCorrectGameCode()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = "UNIQUE123",
            PlayerName = "TestPlayer"
        };

        var gameState = new GameState
        {
            GameCode = "UNIQUE123",
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)>()
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockGrainFactory.Verify(f => f.GetGrain<IGameGrain>("UNIQUE123", null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsJoinGameOnGrain()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "TestPlayer"
        };

        var gameState = new GameState
        {
            GameCode = "ABC123",
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)>()
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockGameGrain.Verify(g => g.JoinGame(command), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReturnsGameDetailsResponseFromGameState()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "NewPlayer"
        };

        var gameState = new GameState
        {
            GameCode = "ABC123",
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)>
            {
                ("player1", "Player1"),
                ("player2", "NewPlayer")
            },
            Password = "password123",
            Settings = new GameSettings
            {
                MaxPlayers = 8,
                NumberOfRounds = 3,
                Category = "Movies"
            }
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ABC123", result.GameCode);
        Assert.Equal(2, result.Players.Count);
        Assert.Contains(result.Players, p => p.Name == "Player1");
        Assert.Contains(result.Players, p => p.Name == "NewPlayer");
        Assert.True(result.IsPasswordProtected);
        Assert.Equal(8, result.Settings.MaxPlayers);
        Assert.Equal(3, result.Settings.NumberOfRounds);
        Assert.Equal("Movies", result.Settings.Category);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_PassesTokenCorrectly()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "TestPlayer"
        };

        var gameState = new GameState
        {
            GameCode = "ABC123",
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)>()
        };

        var cancellationToken = new CancellationToken();

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC123", result.GameCode);
    }

    [Theory]
    [InlineData("GAME01", "Player1")]
    [InlineData("GAME02", "Player2")]
    [InlineData("ABC123", "Alice")]
    [InlineData("XYZ789", "Bob")]
    public async Task HandleAsync_WithDifferentCommands_ReturnsCorrectGameCode(string gameCode, string playerName)
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = gameCode,
            PlayerName = playerName
        };

        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)> { ("id1", playerName) }
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(gameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Contains(result.Players, p => p.Name == playerName);
    }

    [Fact]
    public async Task HandleAsync_WithPassword_HandlesPasswordCorrectly()
    {
        // Arrange
        var command = new JoinGameCommand
        {
            GameCode = "ABC123",
            PlayerName = "TestPlayer",
            Password = "mypassword"
        };

        var gameState = new GameState
        {
            GameCode = "ABC123",
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)>(),
            Password = "mypassword"
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.JoinGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ABC123", result.GameCode);
        _mockGameGrain.Verify(g => g.JoinGame(It.Is<JoinGameCommand>(c => c.Password == "mypassword")), Times.Once);
    }
}
