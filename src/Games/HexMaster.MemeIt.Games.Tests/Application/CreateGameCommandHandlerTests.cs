using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Games;
using Moq;

namespace HexMaster.MemeIt.Games.Tests.Application;

public sealed class CreateGameCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_Persists_Game_And_Returns_Result()
    {
        var repositoryMock = new Mock<IGamesRepository>();
        IGame? persistedGame = null;
        repositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()))
            .Callback<IGame, CancellationToken>((game, _) => persistedGame = game)
            .Returns(Task.CompletedTask);

        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        codeGeneratorMock.Setup(gen => gen.Generate()).Returns("ABCDEFGH");

        var now = DateTimeOffset.UtcNow;
        var handler = new CreateGameCommandHandler(repositoryMock.Object, codeGeneratorMock.Object, new FixedTimeProvider(now));

        var command = new CreateGameCommand(Guid.NewGuid(), "Admin", null);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(persistedGame);
        Assert.Equal("ABCDEFGH", result.GameCode);
        Assert.Equal(command.PlayerId, result.AdminPlayerId);
        Assert.Equal(now, result.CreatedAt);
        Assert.Equal("Lobby", result.State.Name);
    }

    [Fact]
    public async Task HandleAsync_Throws_For_Missing_DisplayName()
    {
        var repositoryMock = new Mock<IGamesRepository>();
        var codeGeneratorMock = new Mock<IGameCodeGenerator>();
        var handler = new CreateGameCommandHandler(repositoryMock.Object, codeGeneratorMock.Object, TimeProvider.System);

        var command = new CreateGameCommand(Guid.NewGuid(), "", null);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
        repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<IGame>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
            => _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }
}
