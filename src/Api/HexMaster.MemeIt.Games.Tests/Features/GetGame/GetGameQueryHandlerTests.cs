using HexMaster.MemeIt.Core.DataTransferObjects;
using HexMaster.MemeIt.Games.Abstractions.Grains;
using HexMaster.MemeIt.Games.Features;
using HexMaster.MemeIt.Games.Features.GetGame;
using HexMaster.MemeIt.Games.ValueObjects;
using Moq;
using Orleans;

namespace HexMaster.MemeIt.Games.Tests.Features.GetGame;

public class GetGameQueryHandlerTests
{
    private readonly Mock<IGrainFactory> _mockGrainFactory;
    private readonly Mock<IGameGrain> _mockGameGrain;
    private readonly GetGameQueryHandler _handler;

    public GetGameQueryHandlerTests()
    {
        _mockGrainFactory = new Mock<IGrainFactory>();
        _mockGameGrain = new Mock<IGameGrain>();
        _handler = new GetGameQueryHandler(_mockGrainFactory.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidGameExists_ReturnsSuccessfulOperationResult()
    {
        // Arrange
        var query = new GetGameQuery("ABC123");
        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)> { ("player1", "TestPlayer") },
            Status = GameStatus.Waiting.Id,
            Settings = new GameSettings()
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResponseObject);
        Assert.Equal("ABC123", result.ResponseObject.GameCode);
    }

    [Fact]
    public async Task HandleAsync_UninitializedGame_ReturnsFailedOperationResult()
    {
        // Arrange
        var query = new GetGameQuery("ABC123");
        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)>(),
            Status = GameStatus.Uninitialized.Id
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.ResponseObject);
    }

    [Fact]
    public async Task HandleAsync_CallsGetGrainWithCorrectGameId()
    {
        // Arrange
        var query = new GetGameQuery("UNIQUE123");
        var gameState = new GameState
        {
            GameCode = "UNIQUE123",
            Players = new List<(string Id, string Name)>(),
            Status = GameStatus.Waiting.Id
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _mockGrainFactory.Verify(f => f.GetGrain<IGameGrain>("UNIQUE123", null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CallsGetCurrentOnGrain()
    {
        // Arrange
        var query = new GetGameQuery("ABC123");
        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)>(),
            Status = GameStatus.Active.Id
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _mockGameGrain.Verify(g => g.GetCurrent(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidGame_ReturnsCorrectGameDetailsResponse()
    {
        // Arrange
        var query = new GetGameQuery("ABC123");
        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)>
            {
                ("player1", "Alice"),
                ("player2", "Bob")
            },
            Status = GameStatus.Active.Id,
            Password = "secret",
            Settings = new GameSettings
            {
                MaxPlayers = 8,
                NumberOfRounds = 3,
                Category = "Movies"
            }
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResponseObject);
        Assert.Equal("ABC123", result.ResponseObject.GameCode);
        Assert.Equal(2, result.ResponseObject.Players.Count);
        Assert.Contains(result.ResponseObject.Players, p => p.Name == "Alice");
        Assert.Contains(result.ResponseObject.Players, p => p.Name == "Bob");
        Assert.True(result.ResponseObject.IsPasswordProtected);
        Assert.Equal(8, result.ResponseObject.Settings.MaxPlayers);
        Assert.Equal(3, result.ResponseObject.Settings.NumberOfRounds);
        Assert.Equal("Movies", result.ResponseObject.Settings.Category);
    }

    [Theory]
    [InlineData(GameStatusName.Waiting)]
    [InlineData(GameStatusName.Active)]
    [InlineData(GameStatusName.Finished)]
    public async Task HandleAsync_WithNonUninitializedStatus_ReturnsSuccess(string status)
    {
        // Arrange
        var query = new GetGameQuery("ABC123");
        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)>(),
            Status = status
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResponseObject);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_PassesTokenCorrectly()
    {
        // Arrange
        var query = new GetGameQuery("ABC123");
        var gameState = new GameState
        {
            GameCode = "ABC123",
            Players = new List<(string Id, string Name)>(),
            Status = GameStatus.Waiting.Id
        };

        var cancellationToken = new CancellationToken();

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(query.GameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(query, cancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResponseObject);
    }

    [Theory]
    [InlineData("GAME01")]
    [InlineData("GAME02")]
    [InlineData("ABC123")]
    [InlineData("XYZ789")]
    public async Task HandleAsync_WithDifferentGameIds_ReturnsCorrectGameCode(string gameId)
    {
        // Arrange
        var query = new GetGameQuery(gameId);
        var gameState = new GameState
        {
            GameCode = gameId,
            Players = new List<(string Id, string Name)>(),
            Status = GameStatus.Waiting.Id
        };

        _mockGrainFactory
            .Setup(f => f.GetGrain<IGameGrain>(gameId, null))
            .Returns(_mockGameGrain.Object);

        _mockGameGrain
            .Setup(g => g.GetCurrent())
            .ReturnsAsync(gameState);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ResponseObject);
        Assert.Equal(gameId, result.ResponseObject.GameCode);
    }
}
