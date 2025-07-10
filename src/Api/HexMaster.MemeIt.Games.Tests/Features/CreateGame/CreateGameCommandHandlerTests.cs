using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Moq;
using Orleans;

namespace HexMaster.MemeIt.Games.Tests.Features.CreateGame;

public class CreateGameCommandHandlerTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IGameGrain> _mockGameGrain;
    private readonly CreateGameCommandHandler _handler;

    public CreateGameCommandHandlerTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockGameGrain = new Mock<IGameGrain>();
        _handler = new CreateGameCommandHandler(_mockGrainFactory.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsCreateGameResponse()
    {
        // Arrange
        var command = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "ABC123",
            Password = "secret"
        };

        var expectedGameState = new GameState
        {
            GameCode = "ABC123",
            Status = GameStatus.Waiting.Id,
            Players = new List<(string Id, string Name)> { ("player1", "TestPlayer") }
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(command.GameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.CreateGame(command))
            .ReturnsAsync(expectedGameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC123", result.GameCode);
    }

    [Fact]
    public async Task HandleAsync_CallsGetGrainWithCorrectGameCode()
    {
        // Arrange
        var command = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "UNIQUE123"
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
            .Setup(g => g.CreateGame(command))
            .ReturnsAsync(gameState);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockGrainFactory.Verify(f => f.GetGrain<IGameGrain>("UNIQUE123", null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsCreateGameOnGrain()
    {
        // Arrange
        var command = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "ABC123"
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
            .Setup(g => g.CreateGame(command))
            .ReturnsAsync(gameState);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockGameGrain.Verify(g => g.CreateGame(command), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_PassesTokenCorrectly()
    {
        // Arrange
        var command = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "ABC123"
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
            .Setup(g => g.CreateGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ABC123", result.GameCode);
    }

    [Theory]
    [InlineData("Player1", "GAME01")]
    [InlineData("Player2", "GAME02")]
    [InlineData("Alice", "ABC123")]
    [InlineData("Bob", "XYZ789")]
    public async Task HandleAsync_WithDifferentCommands_ReturnsCorrectGameCode(string playerName, string gameCode)
    {
        // Arrange
        var command = new CreateGameCommand
        {
            PlayerName = playerName,
            GameCode = gameCode
        };

        var gameState = new GameState
        {
            GameCode = gameCode,
            Status = GameStatus.Waiting.Id,

            Players = new List<(string Id, string Name)>()
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(gameCode, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.CreateGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
    }

    [Fact]
    public async Task HandleAsync_WithPassword_HandlesPasswordCorrectly()
    {
        // Arrange
        var command = new CreateGameCommand
        {
            PlayerName = "TestPlayer",
            GameCode = "ABC123",
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
            .Setup(g => g.CreateGame(command))
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal("ABC123", result.GameCode);
        _mockGameGrain.Verify(g => g.CreateGame(It.Is<CreateGameCommand>(c => c.Password == "mypassword")), Times.Once);
    }
}
