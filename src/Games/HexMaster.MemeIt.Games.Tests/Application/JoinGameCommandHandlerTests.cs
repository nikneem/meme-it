using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;
using HexMaster.MemeIt.Games.Application.Games;
using HexMaster.MemeIt.Games.Domains;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class JoinGameCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Joins_Player_To_Game_And_Persists()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";
        var playerName = "NewPlayer";

        var game = new Game(gameCode, adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        IGame? updatedGame = null;
        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((g, _) => updatedGame = g)
            .Returns(Task.CompletedTask);

        var handler = new JoinGameCommandHandler(repositoryMock.Object);
        var command = new JoinGameCommand(playerId, playerName, gameCode, null);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal(GameState.Lobby, result.State);

        Assert.NotNull(updatedGame);
        Assert.Contains(updatedGame.Players, p => p.PlayerId == playerId && p.DisplayName == playerName);

        repositoryMock.Verify(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Validates_Password_When_Game_Is_Protected()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";
        var password = "secret123";

        var game = new Game(gameCode, adminId, password: password);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new JoinGameCommandHandler(repositoryMock.Object);
        var command = new JoinGameCommand(playerId, "Player", gameCode, "wrongpassword");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("password", exception.Message, StringComparison.OrdinalIgnoreCase);

        repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_Found()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new JoinGameCommandHandler(repositoryMock.Object);
        var command = new JoinGameCommand(Guid.NewGuid(), "Player", "NOTFOUND", null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Player_Already_Joined()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerId, "FirstTime", null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new JoinGameCommandHandler(repositoryMock.Object);
        var command = new JoinGameCommand(playerId, "SecondTime", gameCode, null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("already joined", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_GameCode_Is_Empty()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        var handler = new JoinGameCommandHandler(repositoryMock.Object);
        var command = new JoinGameCommand(Guid.NewGuid(), "Player", "", null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_PlayerName_Is_Empty()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        var handler = new JoinGameCommandHandler(repositoryMock.Object);
        var command = new JoinGameCommand(Guid.NewGuid(), "", "GAMECODE", null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }
}
