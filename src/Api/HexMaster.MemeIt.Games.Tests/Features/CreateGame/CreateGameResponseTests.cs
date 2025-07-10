using HexMaster.MemeIt.Games.Features.CreateGame;

namespace HexMaster.MemeIt.Games.Tests.Features.CreateGame;

public class CreateGameResponseTests
{
    [Fact]
    public void CreateGameResponse_Constructor_SetsGameCodeCorrectly()
    {
        // Arrange
        var gameCode = "ABC123";

        // Act
        var response = new CreateGameResponse(gameCode);

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
        // Act
        var response = new CreateGameResponse(gameCode);

        // Assert
        Assert.Equal(gameCode, response.GameCode);
    }

    [Fact]
    public void CreateGameResponse_WithEmptyGameCode_AcceptsEmptyString()
    {
        // Arrange
        var gameCode = "";

        // Act
        var response = new CreateGameResponse(gameCode);

        // Assert
        Assert.Equal(gameCode, response.GameCode);
    }

    [Fact]
    public void CreateGameResponse_IsRecord_SupportsEquality()
    {
        // Arrange
        var response1 = new CreateGameResponse("ABC123");
        var response2 = new CreateGameResponse("ABC123");
        var response3 = new CreateGameResponse("XYZ789");

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.NotEqual(response1, response3);
    }

    [Fact]
    public void CreateGameResponse_GameCode_IsAccessible()
    {
        // Arrange
        var gameCode = "ABC123";
        var response = new CreateGameResponse(gameCode);

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
        var response = new CreateGameResponse(gameCode);

        // Act
        var toString = response.ToString();

        // Assert
        Assert.Contains(gameCode, toString);
    }
}
