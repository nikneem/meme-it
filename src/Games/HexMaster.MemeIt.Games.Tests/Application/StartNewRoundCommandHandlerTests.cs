using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Games.StartNewRound;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class StartNewRoundCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Starts_New_Round_Successfully()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var gameCode = "TEST1234";
        var game = new Game(gameCode, adminId, password: null);
        game.NextRound(); // Start round 1

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var daprClientMock = new Mock<DaprClient>();
        var scheduledTaskServiceMock = new Mock<IScheduledTaskService>();
        var loggerMock = new Mock<ILogger<StartNewRoundCommandHandler>>();

        var handler = new StartNewRoundCommandHandler(
            repositoryMock.Object,
            daprClientMock.Object,
            scheduledTaskServiceMock.Object,
            loggerMock.Object);

        var command = new StartNewRoundCommand(gameCode, 2);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(2, result.RoundNumber);

        repositoryMock.Verify(r => r.UpdateAsync(game, It.IsAny<CancellationToken>()), Times.Once);
        scheduledTaskServiceMock.Verify(
            s => s.ScheduleCreativePhaseEnded(gameCode, 2, 60),
            Times.Once);
        daprClientMock.Verify(
            d => d.PublishEventAsync(
                DaprConstants.PubSubName,
                DaprConstants.Topics.RoundStarted,
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_Found()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new StartNewRoundCommandHandler(
            repositoryMock.Object,
            new Mock<DaprClient>().Object,
            new Mock<IScheduledTaskService>().Object,
            new Mock<ILogger<StartNewRoundCommandHandler>>().Object);

        var command = new StartNewRoundCommand("INVALID", 2);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Round_Number_Mismatch()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = new Game("TEST1234", adminId, password: null);
        game.NextRound(); // Current round is 1

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new StartNewRoundCommandHandler(
            repositoryMock.Object,
            new Mock<DaprClient>().Object,
            new Mock<IScheduledTaskService>().Object,
            new Mock<ILogger<StartNewRoundCommandHandler>>().Object);

        var command = new StartNewRoundCommand("TEST1234", 5); // Skipping rounds

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("Cannot start round", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Command_Is_Null()
    {
        // Arrange
        var handler = new StartNewRoundCommandHandler(
            new Mock<IGamesRepository>().Object,
            new Mock<DaprClient>().Object,
            new Mock<IScheduledTaskService>().Object,
            new Mock<ILogger<StartNewRoundCommandHandler>>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}
