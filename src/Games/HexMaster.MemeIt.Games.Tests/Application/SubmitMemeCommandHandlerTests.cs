using HexMaster.MemeIt.Games.Abstractions.Application.Commands;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games.EndCreativePhase;
using HexMaster.MemeIt.Games.Application.Games.SubmitMeme;
using HexMaster.MemeIt.Games.Domains;
using HexMaster.MemeIt.IntegrationEvents.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class SubmitMemeCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Submits_Meme_Successfully()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var gameCode = "TEST1234";
        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerId, "Player1", null);
        game.NextRound();
        
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);
        repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var endCreativePhaseHandlerMock = new Mock<ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>>();
        var loggerMock = new Mock<ILogger<SubmitMemeCommandHandler>>();
        
        var handler = new SubmitMemeCommandHandler(
            repositoryMock.Object,
            endCreativePhaseHandlerMock.Object,
            loggerMock.Object);

        var memeTemplateId = Guid.NewGuid();
        var textFieldId = Guid.NewGuid();
        var command = new SubmitMemeCommand(
            gameCode,
            1,
            playerId,
            memeTemplateId,
            new[] { new MemeTextEntryDto(textFieldId, "Top Text") });

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal(1, result.RoundNumber);
        Assert.Equal(memeTemplateId, result.MemeTemplateId);
        Assert.Equal(1, result.TextEntryCount);
        
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

        var handler = new SubmitMemeCommandHandler(
            repositoryMock.Object,
            new Mock<ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>>().Object,
            new Mock<ILogger<SubmitMemeCommandHandler>>().Object);
        
        var command = new SubmitMemeCommand("INVALID", 1, Guid.NewGuid(), Guid.NewGuid(), Array.Empty<MemeTextEntryDto>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Player_Not_In_Game()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var nonPlayerId = Guid.NewGuid();
        var game = new Game("TEST1234", adminId, password: null);
        game.NextRound();
        
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new SubmitMemeCommandHandler(
            repositoryMock.Object,
            new Mock<ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>>().Object,
            new Mock<ILogger<SubmitMemeCommandHandler>>().Object);
        
        var command = new SubmitMemeCommand("TEST1234", 1, nonPlayerId, Guid.NewGuid(), Array.Empty<MemeTextEntryDto>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not part of this game", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Command_Is_Null()
    {
        // Arrange
        var handler = new SubmitMemeCommandHandler(
            new Mock<IGamesRepository>().Object,
            new Mock<ICommandHandler<EndCreativePhaseCommand, EndCreativePhaseResult>>().Object,
            new Mock<ILogger<SubmitMemeCommandHandler>>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}
