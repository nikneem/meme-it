using Bogus;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;
using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Tests.Domains;

public class GameTests
{
    private readonly Faker _faker = new();

    [Fact]
    public void Constructor_ShouldInitializeLobbyState()
    {
        var game = CreateGame();

        Assert.Equal(GameState.Lobby, game.State);
        Assert.Empty(game.Players);
        Assert.Empty(game.Rounds);
    }

    [Fact]
    public void AddPlayer_ShouldAllowPlayerWithoutPassword()
    {
        var playerId = Guid.NewGuid();
        var displayName = _faker.Person.FullName;
        var game = CreateGame();

        game.AddPlayer(playerId, displayName);

        Assert.Contains(game.Players, p => p.PlayerId == playerId && p.DisplayName == displayName);
    }

    [Fact]
    public void AddPlayer_ShouldRejectWhenPasswordIncorrect()
    {
        var game = CreateGame(password: "secret123");
        var playerId = Guid.NewGuid();

        var ex = Assert.Throws<InvalidOperationException>(() => game.AddPlayer(playerId, "Jane", passwordAttempt: "wrong"));
        Assert.Contains("Invalid password", ex.Message);
        Assert.Empty(game.Players);
    }

    [Fact]
    public void AddPlayer_ShouldRejectDuplicatePlayer()
    {
        var playerId = Guid.NewGuid();
        var game = CreateGame();
        game.AddPlayer(playerId, "Jane");

        var ex = Assert.Throws<InvalidOperationException>(() => game.AddPlayer(playerId, "Jane"));
        Assert.Contains("already joined", ex.Message);
    }

    [Fact]
    public void RemovePlayer_ShouldRemovePlayerEntries()
    {
        var playerId = Guid.NewGuid();
        var game = CreateGame();
        game.AddPlayer(playerId, "Jane");
        var round = game.NextRound();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<IMemeTextEntry>());
        game.AddMemeSubmission(round.RoundNumber, submission);

        game.RemovePlayer(playerId);

        Assert.DoesNotContain(game.Players, p => p.PlayerId == playerId);
        Assert.All(game.Rounds, r => Assert.DoesNotContain(r.Submissions, s => s.PlayerId == playerId));
    }

    [Fact]
    public void RemovePlayer_ShouldNotRemoveAdmin()
    {
        var adminId = Guid.NewGuid();
        var game = CreateGame(adminId: adminId);

        var ex = Assert.Throws<InvalidOperationException>(() => game.RemovePlayer(adminId));
        Assert.Contains("admin", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NextRound_ShouldCreateNewRoundAndSetInProgress()
    {
        var game = CreateGame();

        var round = game.NextRound();

        Assert.Equal(1, round.RoundNumber);
        Assert.Equal(GameState.InProgress, game.State);
        Assert.Single(game.Rounds);
    }

    [Fact]
    public void NextRound_ShouldPreventBeyondTarget()
    {
        var game = CreateGame(roundTarget: 1);
        game.NextRound();

        Assert.Throws<InvalidOperationException>(() => game.NextRound());
    }

    [Fact]
    public void AddMemeSubmission_ShouldRequireExistingRound()
    {
        var game = CreateGame();
        game.AddPlayer(Guid.NewGuid(), "Player");

        var submission = new MemeSubmission(game.Players.First().PlayerId, Guid.NewGuid(), Array.Empty<IMemeTextEntry>());

        var ex = Assert.Throws<InvalidOperationException>(() => game.AddMemeSubmission(1, submission));
        Assert.Contains("Submissions can only", ex.Message);
    }

    [Fact]
    public void AddMemeSubmission_ShouldRejectNonPlayer()
    {
        var game = CreateGame();
        game.NextRound();
        var submission = new MemeSubmission(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<IMemeTextEntry>());

        var ex = Assert.Throws<InvalidOperationException>(() => game.AddMemeSubmission(1, submission));
        Assert.Contains("must be part of the game", ex.Message);
    }

    [Fact]
    public void AddMemeSubmission_ShouldUpsertPerPlayer()
    {
        var playerId = Guid.NewGuid();
        var game = CreateGame();
        game.AddPlayer(playerId, "Player");
        game.NextRound();

        var textEntry = new MemeTextEntry(Guid.NewGuid(), "Hello");
        game.AddMemeSubmission(1, new MemeSubmission(playerId, Guid.NewGuid(), new[] { textEntry }));

        var updatedEntry = new MemeTextEntry(textEntry.TextFieldId, "World");
        game.AddMemeSubmission(1, new MemeSubmission(playerId, Guid.NewGuid(), new[] { updatedEntry }));

        var round = game.Rounds.Single();
        var submission = round.Submissions.Single();
        Assert.Equal("World", submission.TextEntries.Single().Value);
    }

    [Fact]
    public void ChangeState_ShouldRespectTransitions()
    {
        var game = CreateGame();
        game.NextRound();

        game.ChangeState(GameState.Scoring);
        Assert.Equal(GameState.Scoring, game.State);

        var ex = Assert.Throws<InvalidOperationException>(() => game.ChangeState(GameState.Lobby));
        Assert.Contains("Cannot transition", ex.Message);
    }

    [Fact]
    public void Finish_ShouldTransitionToCompleted()
    {
        var game = CreateGame();
        game.NextRound();

        game.Finish();

        Assert.Equal(GameState.Completed, game.State);
    }

    [Fact]
    public void SetPlayerReady_ShouldUpdatePlayerReadyStatus()
    {
        var playerId = Guid.NewGuid();
        var game = CreateGame();
        game.AddPlayer(playerId, "Player");

        game.SetPlayerReady(playerId, true);

        var player = game.Players.First(p => p.PlayerId == playerId);
        Assert.True(player.IsReady);
    }

    [Fact]
    public void SetPlayerReady_ShouldRejectIfNotInLobby()
    {
        var playerId = Guid.NewGuid();
        var game = CreateGame();
        game.AddPlayer(playerId, "Player");
        game.NextRound();

        var ex = Assert.Throws<InvalidOperationException>(() => game.SetPlayerReady(playerId, true));
        Assert.Contains("lobby", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetPlayerReady_ShouldRejectNonExistentPlayer()
    {
        var game = CreateGame();

        var ex = Assert.Throws<InvalidOperationException>(() => game.SetPlayerReady(Guid.NewGuid(), true));
        Assert.Contains("not part of this game", ex.Message);
    }

    [Fact]
    public void AreAllPlayersReady_ShouldReturnFalseWhenNoPlayers()
    {
        var game = CreateGame();

        Assert.False(game.AreAllPlayersReady());
    }

    [Fact]
    public void AreAllPlayersReady_ShouldReturnFalseWhenSomeNotReady()
    {
        var game = CreateGame();
        var player1 = Guid.NewGuid();
        var player2 = Guid.NewGuid();
        game.AddPlayer(player1, "Player1");
        game.AddPlayer(player2, "Player2");
        game.SetPlayerReady(player1, true);

        Assert.False(game.AreAllPlayersReady());
    }

    [Fact]
    public void AreAllPlayersReady_ShouldReturnTrueWhenAllReady()
    {
        var game = CreateGame();
        var player1 = Guid.NewGuid();
        var player2 = Guid.NewGuid();
        game.AddPlayer(player1, "Player1");
        game.AddPlayer(player2, "Player2");
        game.SetPlayerReady(player1, true);
        game.SetPlayerReady(player2, true);

        Assert.True(game.AreAllPlayersReady());
    }

    [Fact]
    public void GetRound_ShouldReturnRoundWhenExists()
    {
        var game = CreateGame();
        var round = game.NextRound();

        var result = game.GetRound(round.RoundNumber);

        Assert.NotNull(result);
        Assert.Equal(round.RoundNumber, result.RoundNumber);
    }

    [Fact]
    public void GetRound_ShouldReturnNullWhenNotExists()
    {
        var game = CreateGame();

        var result = game.GetRound(99);

        Assert.Null(result);
    }

    [Fact]
    public void GetRandomUnratedSubmissionForRound_ShouldThrowWhenRoundNotFound()
    {
        var game = CreateGame();

        var ex = Assert.Throws<InvalidOperationException>(() => game.GetRandomUnratedSubmissionForRound(99));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void AddScore_ShouldThrowWhenRoundNotFound()
    {
        var game = CreateGame();

        var ex = Assert.Throws<InvalidOperationException>(() => game.AddScore(99, Guid.NewGuid(), Guid.NewGuid(), 5));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void AddScore_ShouldDelegateToGameRound()
    {
        var playerId = Guid.NewGuid();
        var game = CreateGame();
        game.AddPlayer(playerId, "Player");
        var round = game.NextRound();
        var submission = new MemeSubmission(playerId, Guid.NewGuid(), Array.Empty<IMemeTextEntry>());
        game.AddMemeSubmission(round.RoundNumber, submission);

        game.AddScore(round.RoundNumber, submission.SubmissionId, Guid.NewGuid(), 4);

        var updatedRound = game.GetRound(round.RoundNumber);
        var updatedSubmission = updatedRound!.Submissions.First();
        Assert.Single(updatedSubmission.Scores);
    }

    [Fact]
    public void MarkCreativePhaseEnded_ShouldMarkRoundPhaseEnded()
    {
        var game = CreateGame();
        var round = game.NextRound();

        game.MarkCreativePhaseEnded(round.RoundNumber);

        var updatedRound = game.GetRound(round.RoundNumber);
        Assert.True(updatedRound!.HasCreativePhaseEnded);
    }

    [Fact]
    public void MarkCreativePhaseEnded_ShouldThrowWhenRoundNotFound()
    {
        var game = CreateGame();

        var ex = Assert.Throws<InvalidOperationException>(() => game.MarkCreativePhaseEnded(99));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyGameCode()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateGame(gameCode: ""));
        Assert.Contains("Game code must be provided", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidGameCodeLength()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateGame(gameCode: "SHORT"));
        Assert.Contains("must be 8 characters long", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyAdminId()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateGame(adminId: Guid.Empty));
        Assert.Contains("non-empty guid", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldNormalizeGameCodeToUpperCase()
    {
        var game = CreateGame(gameCode: "abc12345");

        Assert.Equal("ABC12345", game.GameCode);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithInitialPlayers()
    {
        var player1 = new GamePlayer(Guid.NewGuid(), "Player1", isReady: true);
        var player2 = new GamePlayer(Guid.NewGuid(), "Player2", isReady: false);
        var initialPlayers = new[] { player1, player2 };

        var game = new Game(
            "ABCD1234",
            Guid.NewGuid(),
            initialPlayers: initialPlayers,
            createdAt: DateTimeOffset.UtcNow);

        Assert.Equal(2, game.Players.Count);
        Assert.Contains(game.Players, p => p.PlayerId == player1.PlayerId);
        Assert.Contains(game.Players, p => p.PlayerId == player2.PlayerId);
    }

    private Game CreateGame(
        string? gameCode = null,
        Guid? adminId = null,
        string? password = null,
        int roundTarget = 5)
    {
        return new Game(
            gameCode ?? _faker.Random.AlphaNumeric(8).ToUpperInvariant(),
            adminId ?? Guid.NewGuid(),
            password,
            createdAt: DateTimeOffset.UtcNow,
            roundTarget: roundTarget);
    }
}
