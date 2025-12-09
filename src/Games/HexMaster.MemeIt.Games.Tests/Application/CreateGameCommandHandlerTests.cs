using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Games.CreateGame;
using HexMaster.MemeIt.Games.Application.Observability;
using HexMaster.MemeIt.Games.Constants;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class CreateGameCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Persists_Game_And_Returns_Result()
    {
        var repositoryMock = new Mock<IGamesRepository>();
        IGame? persistedGame = null;
        repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((game, _) => persistedGame = game)
            .Returns(Task.CompletedTask);

        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        codeGeneratorMock.Setup(gen => gen.Generate()).Returns("ABCDEFGH");

        var meterFactory = new Mock<System.Diagnostics.Metrics.IMeterFactory>();
        meterFactory.Setup(m => m.Create(It.IsAny<System.Diagnostics.Metrics.MeterOptions>()))
            .Returns(new System.Diagnostics.Metrics.Meter("Test"));
        var metrics = new GamesMetrics(meterFactory.Object);

        var daprClientMock = new Mock<DaprClient>();

        var now = DateTimeOffset.UtcNow;
        var handler = new CreateGameCommandHandler(repositoryMock.Object, codeGeneratorMock.Object, new FixedTimeProvider(now), metrics, daprClientMock.Object);

        var command = new CreateGameCommand(Guid.NewGuid(), "Admin", null, null);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(persistedGame);
        Assert.Equal("ABCDEFGH", result.GameCode);
        Assert.Equal(command.PlayerId, result.AdminPlayerId);
        Assert.Equal(now, result.CreatedAt);
        Assert.Equal("Lobby", result.State.Name);
    }

    [Fact]
    public async Task HandleAsync_Throws_For_Missing_DisplayName()
    {
        var repositoryMock = new Mock<IGamesRepository>();
        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        var meterFactory = new Mock<System.Diagnostics.Metrics.IMeterFactory>();
        meterFactory.Setup(m => m.Create(It.IsAny<System.Diagnostics.Metrics.MeterOptions>()))
            .Returns(new System.Diagnostics.Metrics.Meter("Test"));
        var metrics = new GamesMetrics(meterFactory.Object);
        var daprClientMock = new Mock<DaprClient>();
        var handler = new CreateGameCommandHandler(repositoryMock.Object, codeGeneratorMock.Object, TimeProvider.System, metrics, daprClientMock.Object);

        var command = new CreateGameCommand(Guid.NewGuid(), "", null, null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
        repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Publishes_NewGameStartedEvent_When_PreviousGameCode_Provided()
    {
        // Arrange
        var previousGameCode = "OLDGAME1";
        var newGameCode = "NEWGAME2";
        var adminId = Guid.NewGuid();
        var adminDisplayName = "AdminPlayer";

        var previousGame = new Game(previousGameCode, adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(previousGameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previousGame);

        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        codeGeneratorMock.Setup(gen => gen.Generate()).Returns(newGameCode);

        var meterFactory = new Mock<System.Diagnostics.Metrics.IMeterFactory>();
        meterFactory.Setup(m => m.Create(It.IsAny<System.Diagnostics.Metrics.MeterOptions>()))
            .Returns(new System.Diagnostics.Metrics.Meter("Test"));
        var metrics = new GamesMetrics(meterFactory.Object);

        var daprClientMock = new Mock<DaprClient>();

        var handler = new CreateGameCommandHandler(
            repositoryMock.Object,
            codeGeneratorMock.Object,
            TimeProvider.System,
            metrics,
            daprClientMock.Object);

        var command = new CreateGameCommand(adminId, adminDisplayName, null, previousGameCode);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(newGameCode, result.GameCode);

        daprClientMock.Verify(
            d => d.PublishEventAsync(
                DaprConstants.PubSubName,
                DaprConstants.Topics.NewGameStarted,
                It.Is<object>(e =>
                    e.GetType().GetProperty("PreviousGameCode")!.GetValue(e)!.ToString() == previousGameCode &&
                    e.GetType().GetProperty("NewGameCode")!.GetValue(e)!.ToString() == newGameCode &&
                    e.GetType().GetProperty("InitiatedByPlayerName")!.GetValue(e)!.ToString() == adminDisplayName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Does_Not_Publish_Event_When_PreviousGameCode_Not_Found()
    {
        // Arrange
        var previousGameCode = "NONEXIST";
        var newGameCode = "NEWGAME3";
        var adminId = Guid.NewGuid();

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(previousGameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        codeGeneratorMock.Setup(gen => gen.Generate()).Returns(newGameCode);

        var meterFactory = new Mock<System.Diagnostics.Metrics.IMeterFactory>();
        meterFactory.Setup(m => m.Create(It.IsAny<System.Diagnostics.Metrics.MeterOptions>()))
            .Returns(new System.Diagnostics.Metrics.Meter("Test"));
        var metrics = new GamesMetrics(meterFactory.Object);

        var daprClientMock = new Mock<DaprClient>();

        var handler = new CreateGameCommandHandler(
            repositoryMock.Object,
            codeGeneratorMock.Object,
            TimeProvider.System,
            metrics,
            daprClientMock.Object);

        var command = new CreateGameCommand(adminId, "AdminPlayer", null, previousGameCode);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(newGameCode, result.GameCode);

        daprClientMock.Verify(
            d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Does_Not_Publish_Event_When_PreviousGameCode_Is_Null()
    {
        // Arrange
        var newGameCode = "NEWGAME4";
        var adminId = Guid.NewGuid();

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        codeGeneratorMock.Setup(gen => gen.Generate()).Returns(newGameCode);

        var meterFactory = new Mock<System.Diagnostics.Metrics.IMeterFactory>();
        meterFactory.Setup(m => m.Create(It.IsAny<System.Diagnostics.Metrics.MeterOptions>()))
            .Returns(new System.Diagnostics.Metrics.Meter("Test"));
        var metrics = new GamesMetrics(meterFactory.Object);

        var daprClientMock = new Mock<DaprClient>();

        var handler = new CreateGameCommandHandler(
            repositoryMock.Object,
            codeGeneratorMock.Object,
            TimeProvider.System,
            metrics,
            daprClientMock.Object);

        var command = new CreateGameCommand(adminId, "AdminPlayer", null, null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(newGameCode, result.GameCode);

        daprClientMock.Verify(
            d => d.PublishEventAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
            => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
