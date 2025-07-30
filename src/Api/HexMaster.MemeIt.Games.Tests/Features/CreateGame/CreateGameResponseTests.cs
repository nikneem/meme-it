using HexMaster.MemeIt.Games.Features.CreateGame;
using HexMaster.MemeIt.Games.Features;

namespace HexMaster.MemeIt.Games.Tests.Features.CreateGame;

public class CreateGameResponseTests
{
    [Fact]
    public void CreateGameResponse_Constructor_SetsGameCodeCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";
        var status = "Waiting";
        var players = new List<PlayerResponse>();
        var playerId = "player1";
        var isPasswordProtected = false;
        var settings = new GameSettingsResponse(4, 3, "Movies");

        // Act
        var response = new CreateGameResponse(gameCode, status, players, playerId, isPasswordProtected, settings);

        // Assert
        Assert.Equal(gameCode, response.GameCode);
    }

    [Theory]
    [InlineData("GAME01")]
    [InlineData("XYZ999")]
    [InlineData("TEST123")]
    [InlineData("A1B2C3")]
    public void CreateGameResponse_AcceptsVariousGameCodes(string gameCode)
    {
        // Arrange
        var status = "Waiting";
        var players = new List<PlayerResponse>();
        var playerId = "player1";
        var isPasswordProtected = false;
        var settings = new GameSettingsResponse(4, 3, "Movies");

        // Act
        var response = new CreateGameResponse(gameCode, status, players, playerId, isPasswordProtected, settings);

        // Assert
        Assert.Equal(gameCode, response.GameCode);
    }

    [Fact]
    public void CreateGameResponse_WithEmptyGameCode_AcceptsEmptyString()
    {
        // Arrange
        var gameCode = "";
        var status = "Waiting";
        var players = new List<PlayerResponse>();
        var playerId = "player1";
        var isPasswordProtected = false;
        var settings = new GameSettingsResponse(4, 3, "Movies");

        // Act
        var response = new CreateGameResponse(gameCode, status, players, playerId, isPasswordProtected, settings);

        // Assert
        Assert.Equal(gameCode, response.GameCode);
    }

    [Fact]
    public void CreateGameResponse_IsRecord_SupportsEquality()
    {
        // Arrange
        var status = "Waiting";
        var players = new List<PlayerResponse>();
        var playerId = "player1";
        var isPasswordProtected = false;
        var settings = new GameSettingsResponse(4, 3, "Movies");
        
        var response1 = new CreateGameResponse("ABC123", status, players, playerId, isPasswordProtected, settings);
        var response2 = new CreateGameResponse("ABC123", status, players, playerId, isPasswordProtected, settings);
        var response3 = new CreateGameResponse("XYZ789", status, players, playerId, isPasswordProtected, settings);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.NotEqual(response1, response3);
    }

    [Fact]
    public void CreateGameResponse_GameCode_IsAccessible()
    {
        // Arrange
        var gameCode = "ABC123";
        var status = "Waiting";
        var players = new List<PlayerResponse>();
        var playerId = "player1";
        var isPasswordProtected = false;
        var settings = new GameSettingsResponse(4, 3, "Movies");
        var response = new CreateGameResponse(gameCode, status, players, playerId, isPasswordProtected, settings);

        // Act
        var extractedGameCode = response.GameCode;

        // Assert
        Assert.Equal(gameCode, extractedGameCode);
    }

    [Fact]
    public void CreateGameResponse_ToString_ContainsGameCode()
    {
        // Arrange
        var gameCode = "ABC123";
        var status = "Waiting";
        var players = new List<PlayerResponse>();
        var playerId = "player1";
        var isPasswordProtected = false;
        var settings = new GameSettingsResponse(4, 3, "Movies");
        var response = new CreateGameResponse(gameCode, status, players, playerId, isPasswordProtected, settings);

        // Act
        var toString = response.ToString();

        // Assert
        Assert.Contains(gameCode, toString);
    }
}
