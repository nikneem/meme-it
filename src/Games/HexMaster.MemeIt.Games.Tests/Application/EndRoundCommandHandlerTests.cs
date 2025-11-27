using Bogus;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;
using HexMaster.MemeIt.Games.Application.Games.EndRound;
using HexMaster.MemeIt.Games.Domains;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public class EndRoundCommandHandlerTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IGamesRepository> _mockRepository;
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly Mock<IScheduledTaskService> _mockScheduledTaskService;
    private readonly Mock<ILogger<EndRoundCommandHandler>> _mockLogger;

    public EndRoundCommandHandlerTests()
    {
        _mockRepository = new Mock<IGamesRepository>();
        _mockDaprClient = new Mock<DaprClient>();
        _mockScheduledTaskService = new Mock<IScheduledTaskService>();
        _mockLogger = new Mock<ILogger<EndRoundCommandHandler>>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenCommandIsNull()
    {
        var handler = new EndRoundCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenGameNotFound()
    {
        var command = new EndRoundCommand(_faker.Random.AlphaNumeric(8), 1);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new EndRoundCommandHandler(
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
        var command = new EndRoundCommand(_faker.Random.AlphaNumeric(8), 1);
        var game = CreateGame(command.GameCode);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new EndRoundCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("Round", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldMarkRoundComplete()
    {
        var command = new EndRoundCommand(_faker.Random.AlphaNumeric(8), 1);
        var game = CreateGame(command.GameCode);
        var playerId = Guid.NewGuid();
        game.AddPlayer(playerId, "Player");
        var round = game.NextRound();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<IMemeTextEntry>());
        game.AddMemeSubmission(round.RoundNumber, submission);
        round.MarkMemeScorePhaseEnded(submission.SubmissionId);

        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new EndRoundCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockLogger.Object);

        var result = await handler.HandleAsync(command);

        Assert.True(result.Success);
        _mockRepository.Verify(r => r.UpdateAsync(game, It.IsAny<CancellationToken>()), Times.Once);
    }

    private Game CreateGame(string gameCode)
    {
        return new Game(
            gameCode,
            Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow);
    }
}
