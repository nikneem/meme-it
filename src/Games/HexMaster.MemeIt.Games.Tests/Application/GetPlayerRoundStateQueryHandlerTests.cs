using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games.GetPlayerRoundState;
using HexMaster.MemeIt.Games.Domains;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class GetPlayerRoundStateQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_Returns_Player_Round_State()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var gameCode = "TEST1234";
        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerId, "Player1", null);
        var round = game.NextRound();
        
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new GetPlayerRoundStateQueryHandler(repositoryMock.Object);
        var query = new GetPlayerRoundStateQuery(gameCode, playerId);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal(round.RoundNumber, result.RoundNumber);
        Assert.Null(result.SelectedMemeTemplateId); // No submission yet
    }

    [Fact]
    public async Task HandleAsync_Returns_State_With_Submission()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var gameCode = "TEST1234";
        var game = new Game(gameCode, adminId, password: null);
        game.AddPlayer(playerId, "Player1", null);
        var round = game.NextRound();
        
        var memeTemplateId = Guid.NewGuid();
        var submission = new MemeSubmission(playerId, memeTemplateId, Array.Empty<MemeTextEntry>());
        game.AddMemeSubmission(round.RoundNumber, submission);
        
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new GetPlayerRoundStateQueryHandler(repositoryMock.Object);
        var query = new GetPlayerRoundStateQuery(gameCode, playerId);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.Equal(gameCode, result.GameCode);
        Assert.Equal(playerId, result.PlayerId);
        Assert.Equal(memeTemplateId, result.SelectedMemeTemplateId);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Game_Not_Found()
    {
        // Arrange
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IGame?)null);

        var handler = new GetPlayerRoundStateQueryHandler(repositoryMock.Object);
        var query = new GetPlayerRoundStateQuery("INVALID", Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(query));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Player_Not_In_Game()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = new Game("TEST1234", adminId, password: null);
        game.NextRound();
        
        var repositoryMock = new Mock<IGamesRepository>();
        repositoryMock
            .Setup(r => r.GetByGameCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var handler = new GetPlayerRoundStateQueryHandler(repositoryMock.Object);
        var query = new GetPlayerRoundStateQuery("TEST1234", Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.HandleAsync(query));
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Query_Is_Null()
    {
        // Arrange
        var handler = new GetPlayerRoundStateQueryHandler(new Mock<IGamesRepository>().Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => handler.HandleAsync(null!));
    }
}
