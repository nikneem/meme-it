using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games.SelectMemeTemplate;
using HexMaster.MemeIt.Games.Domains;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class SelectMemeTemplateCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Selects_Template_Successfully()
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

        var handler = new SelectMemeTemplateCommandHandler(repositoryMock.Object);

        var memeTemplateId = Guid.NewGuid();
        var command = new SelectMemeTemplateCommand(gameCode, playerId, 1, memeTemplateId);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal(1, result.RoundNumber);
        Assert.Equal(memeTemplateId, result.MemeTemplateId);
        
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

        var handler = new SelectMemeTemplateCommandHandler(repositoryMock.Object);
        var command = new SelectMemeTemplateCommand("INVALID", Guid.NewGuid(), 1, Guid.NewGuid());

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

        var handler = new SelectMemeTemplateCommandHandler(repositoryMock.Object);
        var command = new SelectMemeTemplateCommand("TEST1234", nonPlayerId, 1, Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command));
        Assert.Contains("not part of this game", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Command_Is_Null()
    {
        // Arrange
        var handler = new SelectMemeTemplateCommandHandler(new Mock<IGamesRepository>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}
