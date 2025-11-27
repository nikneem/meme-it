using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games.EndScorePhase;
using HexMaster.MemeIt.Games.Application.Games.RateMeme;
using HexMaster.MemeIt.Games.Domains;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class RateMemeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Rates_Meme_Successfully()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var gameCode = "TEST1234";
        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(player1Id, "Player1", null);
        game.AddPlayer(player2Id, "Player2", null);
        var round = game.NextRound();

        // Player 1 submits a meme
        var memeSubmission = new MemeSubmission(player1Id, Guid.NewGuid(), Array.Empty<MemeTextEntry>());
        game.AddMemeSubmission(round.RoundNumber, memeSubmission);
        game.MarkCreativePhaseEnded(round.RoundNumber);

        var memeId = game.GetRound(round.RoundNumber)!.Submissions.First().SubmissionId;

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var endScorePhaseHandlerMock = new Mock<ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>>();
        var loggerMock = new Mock<ILogger<RateMemeCommandHandler>>();

        var handler = new RateMemeCommandHandler(
            repositoryMock.Object,
            endScorePhaseHandlerMock.Object,
            loggerMock.Object);

        var command = new RateMemeCommand(gameCode, round.RoundNumber, memeId, player2Id, 5);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(round.RoundNumber, result.RoundNumber);
        Assert.True(result.Success);

        repositoryMock.Verify(r => r.UpdateAsync(game, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_Found()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new RateMemeCommandHandler(
            repositoryMock.Object,
            new Mock<ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>>().Object,
            new Mock<ILogger<RateMemeCommandHandler>>().Object);

        var command = new RateMemeCommand("INVALID", 1, Guid.NewGuid(), Guid.NewGuid(), 5);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Round_Not_Found()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = new Game("TEST1234", adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new RateMemeCommandHandler(
            repositoryMock.Object,
            new Mock<ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>>().Object,
            new Mock<ILogger<RateMemeCommandHandler>>().Object);

        var command = new RateMemeCommand("TEST1234", 999, Guid.NewGuid(), Guid.NewGuid(), 5);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Command_Is_Null()
    {
        // Arrange
        var handler = new RateMemeCommandHandler(
            new Mock<IGamesRepository>().Object,
            new Mock<ICommandHandler<EndScorePhaseCommand, EndScorePhaseResult>>().Object,
            new Mock<ILogger<RateMemeCommandHandler>>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}
