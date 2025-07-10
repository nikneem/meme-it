using HexMaster.MemeIt.Games.Features.GetGame;

namespace HexMaster.MemeIt.Games.Tests.Features.GetGame;

public class GetGameQueryTests
{
    [Fact]
    public void GetGameQuery_Constructor_SetsGameIdCorrectly()
    {
        // Arrange
        var gameId = "ABC123";

        // Act
        var query = new GetGameQuery(gameId);

        // Assert
        Assert.Equal(gameId, query.GameId);
    }

    [Theory]
    [InlineData("GAME01")]
    [InlineData("XYZ999")]
    [InlineData("TEST123")]
    [InlineData("A1B2C3")]
    public void GetGameQuery_AcceptsVariousGameIds(string gameId)
    {
        // Act
        var query = new GetGameQuery(gameId);

        // Assert
        Assert.Equal(gameId, query.GameId);
    }

    [Fact]
    public void GetGameQuery_WithEmptyGameId_AcceptsEmptyString()
    {
        // Arrange
        var gameId = "";

        // Act
        var query = new GetGameQuery(gameId);

        // Assert
        Assert.Equal(gameId, query.GameId);
    }

    [Fact]
    public void GetGameQuery_IsRecord_SupportsEquality()
    {
        // Arrange
        var query1 = new GetGameQuery("ABC123");
        var query2 = new GetGameQuery("ABC123");
        var query3 = new GetGameQuery("XYZ789");

        // Act & Assert
        Assert.Equal(query1, query2);
        Assert.NotEqual(query1, query3);
    }

    [Fact]
    public void GetGameQuery_ToString_ContainsGameId()
    {
        // Arrange
        var gameId = "ABC123";
        var query = new GetGameQuery(gameId);

        // Act
        var toString = query.ToString();

        // Assert
        Assert.Contains(gameId, toString);
    }

    [Fact]
    public void GetGameQuery_WithNullGameId_HandlesNull()
    {
        // Arrange & Act
        var query = new GetGameQuery(null!);

        // Assert
        Assert.Null(query.GameId);
    }
}
