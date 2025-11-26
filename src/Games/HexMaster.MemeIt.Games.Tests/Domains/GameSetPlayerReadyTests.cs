using HexMaster.MemeIt.Games.Domains;

namespace HexMaster.MemeIt.Games.Tests.Domains;

public sealed class GameSetPlayerReadyTests
{
    [Fact]
    public void SetPlayerReady_Sets_Player_Ready_Status()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(playerId, "Player", null);

        // Act
        game.SetPlayerReady(playerId, true);

        // Assert
        var player = game.Players.First(p => p.PlayerId == playerId);
        Assert.True(player.IsReady);
    }

    [Fact]
    public void SetPlayerReady_Can_Unset_Ready_Status()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(playerId, "Player", null);
        game.SetPlayerReady(playerId, true);

        // Act
        game.SetPlayerReady(playerId, false);

        // Assert
        var player = game.Players.First(p => p.PlayerId == playerId);
        Assert.False(player.IsReady);
    }

    [Fact]
    public void SetPlayerReady_Throws_When_Player_Not_In_Game()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => game.SetPlayerReady(Guid.NewGuid(), true));
        Assert.Contains("not part of this game", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SetPlayerReady_Throws_When_Game_Not_In_Lobby()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(playerId, "Player", null);
        game.NextRound(); // Move to InProgress state

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => game.SetPlayerReady(playerId, true));
        Assert.Contains("lobby", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AreAllPlayersReady_Returns_False_For_Empty_Game()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);

        // Act
        var result = game.AreAllPlayersReady();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AreAllPlayersReady_Returns_False_When_Not_All_Ready()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(player1Id, "Player1", null);
        game.AddPlayer(player2Id, "Player2", null);
        game.SetPlayerReady(player1Id, true);

        // Act
        var result = game.AreAllPlayersReady();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AreAllPlayersReady_Returns_True_When_All_Ready()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(player1Id, "Player1", null);
        game.AddPlayer(player2Id, "Player2", null);
        game.SetPlayerReady(player1Id, true);
        game.SetPlayerReady(player2Id, true);

        // Act
        var result = game.AreAllPlayersReady();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void AreAllPlayersReady_Returns_False_When_Single_Player_Not_Ready()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(playerId, "Player", null);

        // Act
        var result = game.AreAllPlayersReady();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AreAllPlayersReady_Returns_True_When_Single_Player_Ready()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var game = new Game("TESTCODE", adminId, password: null);
        game.AddPlayer(playerId, "Player", null);
        game.SetPlayerReady(playerId, true);

        // Act
        var result = game.AreAllPlayersReady();

        // Assert
        Assert.True(result);
    }
}
