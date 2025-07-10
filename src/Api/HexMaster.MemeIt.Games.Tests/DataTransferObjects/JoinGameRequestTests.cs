using HexMaster.MemeIt.Games.DataTransferObjects;

namespace HexMaster.MemeIt.Games.Tests.DataTransferObjects;

public class JoinGameRequestTests
{
    [Fact]
    public void JoinGameRequest_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var playerName = "TestPlayer";
        var password = "secret";

        // Act
        var request = new JoinGameRequest(gameCode, playerName, password);

        // Assert
        Assert.Equal(gameCode, request.GameCode);
        Assert.Equal(playerName, request.PlayerName);
        Assert.Equal(password, request.Password);
    }

    [Fact]
    public void JoinGameRequest_WithoutPassword_AllowsNullPassword()
    {
        // Arrange
        var gameCode = "ABC123";
        var playerName = "TestPlayer";

        // Act
        var request = new JoinGameRequest(gameCode, playerName, null);

        // Assert
        Assert.Equal(gameCode, request.GameCode);
        Assert.Equal(playerName, request.PlayerName);
        Assert.Null(request.Password);
    }

    [Theory]
    [InlineData("GAME01", "Alice", "pass123")]
    [InlineData("GAME02", "Bob", null)]
    [InlineData("GAME03", "Charlie", "")]
    [InlineData("GAME04", "David", "very-long-password-123")]
    public void JoinGameRequest_AcceptsVariousInputs(string gameCode, string playerName, string? password)
    {
        // Act
        var request = new JoinGameRequest(gameCode, playerName, password);

        // Assert
        Assert.Equal(gameCode, request.GameCode);
        Assert.Equal(playerName, request.PlayerName);
        Assert.Equal(password, request.Password);
    }

    [Fact]
    public void JoinGameRequest_IsRecord_SupportsEquality()
    {
        // Arrange
        var request1 = new JoinGameRequest("ABC123", "TestPlayer", "secret");
        var request2 = new JoinGameRequest("ABC123", "TestPlayer", "secret");
        var request3 = new JoinGameRequest("ABC123", "TestPlayer", "different");

        // Act & Assert
        Assert.Equal(request1, request2);
        Assert.NotEqual(request1, request3);
    }

    [Fact]
    public void JoinGameRequest_ToString_ContainsGameCodeAndPlayerName()
    {
        // Arrange
        var gameCode = "ABC123";
        var playerName = "TestPlayer";
        var request = new JoinGameRequest(gameCode, playerName, "secret");

        // Act
        var toString = request.ToString();

        // Assert
        Assert.Contains(gameCode, toString);
        Assert.Contains(playerName, toString);
    }

    [Fact]
    public void JoinGameRequest_WithEmptyValues_AcceptsEmptyStrings()
    {
        // Arrange & Act
        var request = new JoinGameRequest("", "", "");

        // Assert
        Assert.Equal("", request.GameCode);
        Assert.Equal("", request.PlayerName);
        Assert.Equal("", request.Password);
    }

    [Fact]
    public void JoinGameRequest_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var playerName = "TestPlayer";
        var password = "secret";
        var request = new JoinGameRequest(gameCode, playerName, password);

        // Act
        var (extractedGameCode, extractedPlayerName, extractedPassword) = request;

        // Assert
        Assert.Equal(gameCode, extractedGameCode);
        Assert.Equal(playerName, extractedPlayerName);
        Assert.Equal(password, extractedPassword);
    }
}
