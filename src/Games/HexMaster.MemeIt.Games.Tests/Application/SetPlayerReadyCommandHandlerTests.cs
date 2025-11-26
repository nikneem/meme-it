using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games;
using HexMaster.MemeIt.Games.Domains;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class SetPlayerReadyCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Sets_Player_Ready_And_Returns_AllReady_Status()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(player1Id, "Player1", null);
        game.AddPlayer(player2Id, "Player2", null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        IGame? updatedGame = null;
        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((g, _) => updatedGame = g)
            .Returns(Task.CompletedTask);

        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(player1Id, gameCode, true);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(player1Id, result.PlayerId);
        Assert.True(result.IsReady);
        Assert.False(result.AllPlayersReady); // Not all players are ready yet

        Assert.NotNull(updatedGame);
        var readyPlayer = updatedGame.Players.First(p => p.PlayerId == player1Id);
        Assert.True(readyPlayer.IsReady);
    }

    [Fact]
    public async Task HandleAsync_Returns_True_When_All_Players_Ready()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(player1Id, "Player1", null);
        game.AddPlayer(player2Id, "Player2", null);
        game.SetPlayerReady(player1Id, true);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(player2Id, gameCode, true);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(player2Id, result.PlayerId);
        Assert.True(result.IsReady);
        Assert.True(result.AllPlayersReady); // Now all players are ready
    }

    [Fact]
    public async Task HandleAsync_Can_Unset_Player_Ready()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerId, "Player", null);
        game.SetPlayerReady(playerId, true);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        IGame? updatedGame = null;
        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((g, _) => updatedGame = g)
            .Returns(Task.CompletedTask);

        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(playerId, gameCode, false);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.Equal(playerId, result.PlayerId);
        Assert.False(result.IsReady);

        Assert.NotNull(updatedGame);
        var player = updatedGame.Players.First(p => p.PlayerId == playerId);
        Assert.False(player.IsReady);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Player_Not_In_Game()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(playerId, gameCode, true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("not part of this game", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_Found()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(Guid.NewGuid(), "NOTFOUND", true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_GameCode_Is_Empty()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(Guid.NewGuid(), "", true);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_In_Lobby()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerId, "Player", null);
        game.NextRound(); // Move to InProgress state

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new SetPlayerReadyCommandHandler(repositoryMock.Object);
        var command = new SetPlayerReadyCommand(playerId, gameCode, true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("lobby", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
