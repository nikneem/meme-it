using Bogus;
using HexMaster.MemeIt.Games.Abstractions.Application.Games;
using HexMaster.MemeIt.Games.Abstractions.Application.Queries;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Application.Games.GetGameDetails;
using HexMaster.MemeIt.Games.Domains;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public class GetGameDetailsQueryHandlerTests
{
    private readonly Mock<IGamesRepository> _repositoryMock;
    private readonly IQueryHandler<GetGameDetailsQuery, GetGameDetailsResult> _handler;
    private readonly Faker _faker;

    public GetGameDetailsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IGamesRepository>();
        _handler = new GetGameDetailsQueryHandler(_repositoryMock.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithValidQueryFromPlayer_ShouldReturnGameDetails()
    {
        // Arrange
        var gameCode = "TESTGAME";
        var playerId = Guid.NewGuid();
        var secondPlayerId = Guid.NewGuid();
        var initialPlayers = new[]
        {
            new GamePlayer(playerId, _faker.Person.FullName, false)
        };
        var game = new Game(gameCode, playerId, password: null, initialPlayers: initialPlayers);
        game.AddPlayer(secondPlayerId, _faker.Person.FullName);

        _repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var query = new GetGameDetailsQuery(gameCode, playerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsAdmin);
        Assert.Equal(2, result.Players.Count);
        Assert.Null(result.CurrentRoundInfo); // No rounds started yet
        Assert.Null(result.PlayerSubmission); // No submission yet
        _repositoryMock.Verify(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidQueryFromNonAdmin_ShouldReturnGameDetailsWithoutAdminFlag()
    {
        // Arrange
        var gameCode = "TESTGAME";
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var initialPlayers = new[]
        {
            new GamePlayer(adminId, _faker.Person.FullName, false),
            new GamePlayer(playerId, _faker.Person.FullName, false)
        };
        var game = new Game(gameCode, adminId, password: null, initialPlayers: initialPlayers);

        _repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var query = new GetGameDetailsQuery(gameCode, playerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsAdmin);
        Assert.Equal(2, result.Players.Count);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentGame_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var gameCode = "NOTFOUND";
        var playerId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Game?)null);

        var query = new GetGameDetailsQuery(gameCode, playerId);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(query, CancellationToken.None));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthorizedPlayer_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var gameCode = "TESTGAME";
        var adminId = Guid.NewGuid();
        var unauthorizedPlayerId = Guid.NewGuid();
        var initialPlayers = new[]
        {
            new GamePlayer(adminId, _faker.Person.FullName, false)
        };
        var game = new Game(gameCode, adminId, password: null, initialPlayers: initialPlayers);

        _repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var query = new GetGameDetailsQuery(gameCode, unauthorizedPlayerId);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithNullQuery_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithEmptyGameCode_ShouldThrowArgumentException()
    {
        // Arrange
        var query = new GetGameDetailsQuery("", Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithGameInProgress_ShouldReturnCurrentRoundInfo()
    {
        // Arrange
        var gameCode = "TESTGAME";
        var playerId = Guid.NewGuid();
        var initialPlayers = new[]
        {
            new GamePlayer(playerId, _faker.Person.FullName, false)
        };
        var game = new Game(gameCode, playerId, password: null, initialPlayers: initialPlayers);

        // Start the game and add a round
        var round = game.NextRound();

        _repositoryMock
            .Setup(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(game);

        var query = new GetGameDetailsQuery(gameCode, playerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.CurrentRoundInfo);
        Assert.Equal(1, result.CurrentRoundInfo.RoundNumber);
        Assert.Equal("Creative", result.CurrentRoundInfo.Phase);
        Assert.NotNull(result.CurrentRoundInfo.CreativePhaseEndTime);
        _repositoryMock.Verify(r => r.GetByGameCodeAsync(gameCode, It.IsAny<CancellationToken>()), Times.Once);
    }
}
