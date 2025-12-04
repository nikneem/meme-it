using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Games.StartGame;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class StartGameCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Starts_Game_Successfully()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var gameCode = "TEST1234";
        var game = new Game(gameCode, adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var daprClientMock = new Mock<DaprClient>();
        var scheduledTaskServiceMock = new Mock<IScheduledTaskService>();

        var handler = new StartGameCommandHandler(
            repositoryMock.Object,
            daprClientMock.Object,
            scheduledTaskServiceMock.Object);

        var command = new StartGameCommand(gameCode, adminId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(1, result.RoundNumber);

        repositoryMock.Verify(r => r.UpdateAsync(game, It.IsAny<CancellationToken>()), Times.Once);
        scheduledTaskServiceMock.Verify(
            s => s.ScheduleCreativePhaseEnded(gameCode, 1, 60),
            Times.Once);
        daprClientMock.Verify(
            d => d.PublishEventAsync(
                DaprConstants.PubSubName,
                DaprConstants.Topics.GameStarted,
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

        var handler = new StartGameCommandHandler(
            repositoryMock.Object,
            new Mock<DaprClient>().Object,
            new Mock<IScheduledTaskService>().Object);

        var command = new StartGameCommand("INVALID", Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Not_Admin()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var nonAdminId = Guid.NewGuid();
        var game = new Game("TEST1234", adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new StartGameCommandHandler(
            repositoryMock.Object,
            new Mock<DaprClient>().Object,
            new Mock<IScheduledTaskService>().Object);

        var command = new StartGameCommand("TEST1234", nonAdminId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Command_Is_Null()
    {
        // Arrange
        var handler = new StartGameCommandHandler(
            new Mock<IGamesRepository>().Object,
            new Mock<DaprClient>().Object,
            new Mock<IScheduledTaskService>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}
