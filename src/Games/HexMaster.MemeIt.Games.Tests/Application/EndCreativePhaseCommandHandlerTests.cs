using Bogus;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Games.EndCreativePhase;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents;
using HexMaster.MemeIt.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public class EndCreativePhaseCommandHandlerTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IGamesRepository> _mockRepository;
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly Mock<IScheduledTaskService> _mockScheduledTaskService;
    private readonly Mock<ILogger<EndCreativePhaseCommandHandler>> _mockLogger;

    public EndCreativePhaseCommandHandlerTests()
    {
        _mockRepository = new Mock<IGamesRepository>();
        _mockDaprClient = new Mock<DaprClient>();
        _mockScheduledTaskService = new Mock<IScheduledTaskService>();
        _mockLogger = new Mock<ILogger<EndCreativePhaseCommandHandler>>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenCommandIsNull()
    {
        var handler = new EndCreativePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenGameNotFound()
    {
        var command = new EndCreativePhaseCommand(_faker.Random.AlphaNumeric(8), 1);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new EndCreativePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenRoundNotFound()
    {
        var command = new EndCreativePhaseCommand(_faker.Random.AlphaNumeric(8), 1);
        var game = CreateGame(command.GameCode);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new EndCreativePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("Round", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWhenCreativePhaseAlreadyEnded()
    {
        var command = new EndCreativePhaseCommand(_faker.Random.AlphaNumeric(8), 1);
        var game = CreateGame(command.GameCode);
        var round = game.NextRound();
        game.MarkCreativePhaseEnded(round.RoundNumber);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new EndCreativePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        var result = await handler.HandleAsync(command);

        Assert.False(result.Success);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldEndCreativePhaseAndPublishEvent()
    {
        var command = new EndCreativePhaseCommand(_faker.Random.AlphaNumeric(8), 1);
        var game = CreateGame(command.GameCode);
        var playerId = Guid.NewGuid();
        game.AddPlayer(playerId, "Player");
        var round = game.NextRound();

        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockDaprClient.Setup(d => d.PublishEventAsync(
            It.Is<string>(s => s == DaprConstants.PubSubName),
            It.Is<string>(s => s == DaprConstants.Topics.CreativePhaseEnded),
            It.IsAny<CreativePhaseEndedEvent>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new EndCreativePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        var result = await handler.HandleAsync(command);

        Assert.True(result.Success);
        _mockRepository.Verify(r => r.UpdateAsync(game, It.IsAny<CancellationToken>()), Times.Once);
        _mockDaprClient.Verify(d => d.PublishEventAsync(
            DaprConstants.PubSubName,
            DaprConstants.Topics.CreativePhaseEnded,
            It.IsAny<CreativePhaseEndedEvent>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private Game CreateGame(string gameCode)
    {
        return new Game(
            gameCode,
            Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow);
    }
}
