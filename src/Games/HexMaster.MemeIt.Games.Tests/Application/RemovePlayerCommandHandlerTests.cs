using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games;
using HexMaster.MemeIt.Games.Domains;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class RemovePlayerCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Removes_Player_When_Admin_Makes_Request()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerToRemove = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerToRemove, "PlayerToRemove", null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        IGame? updatedGame = null;
        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((g, _) => updatedGame = g)
            .Returns(Task.CompletedTask);

        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(adminId, gameCode, playerToRemove);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(updatedGame);
        Assert.DoesNotContain(updatedGame.Players, p => p.PlayerId == playerToRemove);

        repositoryMock.Verify(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()), Times.Once);
        repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Throws_UnauthorizedAccessException_When_NonAdmin_Makes_Request()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var nonAdminId = Guid.NewGuid();
        var playerToRemove = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(nonAdminId, "NonAdmin", null);
        game.AddPlayer(playerToRemove, "PlayerToRemove", null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(nonAdminId, gameCode, playerToRemove);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.HandleAsync(command));
        Assert.Contains("admin", exception.Message, StringComparison.OrdinalIgnoreCase);

        repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Throws_InvalidOperationException_When_Trying_To_Remove_Admin()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(adminId, gameCode, adminId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("admin", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cannot be removed", exception.Message, StringComparison.OrdinalIgnoreCase);

        repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Succeeds_Silently_When_Player_Not_In_Game()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerNotInGame = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(adminId, gameCode, playerNotInGame);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert - Should succeed without throwing
        Assert.NotNull(result);
        repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_Found()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(Guid.NewGuid(), "NOTFOUND", Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(command));
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_GameCode_Is_Empty()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(Guid.NewGuid(), "", Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task HandleAsync_Removes_Player_And_Cleans_Their_Submissions()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerToRemove = Guid.NewGuid();
        var gameCode = "TESTCODE";

        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerToRemove, "PlayerToRemove", null);

        // Start a round and add a submission from the player to be removed
        var round = game.NextRound();
        var submission = new MemeSubmission(
            playerToRemove,
            Guid.NewGuid(),
            new[] { new MemeTextEntry(Guid.NewGuid(), "Test text") });
        game.AddMemeSubmission(round.RoundNumber, submission);

        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(repo => repo.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        IGame? updatedGame = null;
        repositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((g, _) => updatedGame = g)
            .Returns(Task.CompletedTask);

        var handler = new RemovePlayerCommandHandler(repositoryMock.Object);
        var command = new RemovePlayerCommand(adminId, gameCode, playerToRemove);

        // Act
        await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(updatedGame);
        Assert.DoesNotContain(updatedGame.Players, p => p.PlayerId == playerToRemove);

        // Verify submissions were cleaned
        var updatedRound = updatedGame.Rounds.FirstOrDefault();
        Assert.NotNull(updatedRound);
        Assert.DoesNotContain(updatedRound.Submissions, s => s.PlayerId == playerToRemove);
    }
}
