using Bogus;
using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;
using HexMaster.MemeIt.Games.Application.Games.EndScorePhase;
using HexMaster.MemeIt.Games.Domains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public class EndScorePhaseCommandHandlerTests
{
    private readonly Faker _faker = new();
    private readonly Mock<IGamesRepository> _mockRepository;
    private readonly Mock<DaprClient> _mockDaprClient;
    private readonly Mock<IScheduledTaskService> _mockScheduledTaskService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<EndScorePhaseCommandHandler>> _mockLogger;

    public EndScorePhaseCommandHandlerTests()
    {
        _mockRepository = new Mock<IGamesRepository>();
        _mockDaprClient = new Mock<DaprClient>();
        _mockScheduledTaskService = new Mock<IScheduledTaskService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<EndScorePhaseCommandHandler>>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenCommandIsNull()
    {
        var handler = new EndScorePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);

        await Assert.ThrowsAsync<ArgumentNullException>(() => handler.HandleAsync(null!));
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenGameNotFound()
    {
        var command = new EndScorePhaseCommand(_faker.Random.AlphaNumeric(8), 1, Guid.NewGuid());
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new EndScorePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowWhenRoundNotFound()
    {
        var command = new EndScorePhaseCommand(_faker.Random.AlphaNumeric(8), 1, Guid.NewGuid());
        var game = CreateGame(command.GameCode);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new EndScorePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("Round", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalseWhenScorePhaseAlreadyEnded()
    {
        var command = new EndScorePhaseCommand(_faker.Random.AlphaNumeric(8), 1, Guid.NewGuid());
        var game = CreateGame(command.GameCode);
        var playerId = Guid.NewGuid();
        game.AddPlayer(playerId, "Player");
        var round = game.NextRound();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<IMemeTextEntry>());
        game.AddMemeSubmission(round.RoundNumber, submission);
        round.MarkMemeScorePhaseEnded(submission.SubmissionId);

        var command2 = new EndScorePhaseCommand(command.GameCode, round.RoundNumber, submission.SubmissionId);
        _mockRepository.Setup(r => r.GetByGameCodeAsync(command2.GameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new EndScorePhaseCommandHandler(
            _mockRepository.Object,
            _mockDaprClient.Object,
            _mockScheduledTaskService.Object,
            _mockServiceProvider.Object,
            _mockLogger.Object);

        var result = await handler.HandleAsync(command2);

        Assert.False(result.Success);
    }

    private Game CreateGame(string gameCode)
    {
        return new Game(
            gameCode,
            Guid.NewGuid(),
            createdAt: DateTimeOffset.UtcNow);
    }
}
