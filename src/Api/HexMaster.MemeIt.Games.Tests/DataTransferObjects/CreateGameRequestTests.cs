using HexMaster.MemeIt.Games.DataTransferObjects;

namespace HexMaster.MemeIt.Games.Tests.DataTransferObjects;

public class CreateGameRequestTests
{
    [Fact]
    public void CreateGameRequest_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var playerName = "TestPlayer";
        var password = "secret";

        // Act
        var request = new CreateGameRequest(playerName, password);

        // Assert
        Assert.Equal(playerName, request.PlayerName);
        Assert.Equal(password, request.Password);
    }

    [Fact]
    public void CreateGameRequest_WithoutPassword_AllowsNullPassword()
    {
        // Arrange
        var playerName = "TestPlayer";

        // Act
        var request = new CreateGameRequest(playerName, null);

        // Assert
        Assert.Equal(playerName, request.PlayerName);
        Assert.Null(request.Password);
    }

    [Theory]
    [InlineData("Alice", "pass123")]
    [InlineData("Bob", null)]
    [InlineData("Charlie", "")]
    [InlineData("David", "very-long-password-123")]
    public void CreateGameRequest_AcceptsVariousInputs(string playerName, string? password)
    {
        // Act
        var request = new CreateGameRequest(playerName, password);

        // Assert
        Assert.Equal(playerName, request.PlayerName);
        Assert.Equal(password, request.Password);
    }

    [Fact]
    public void CreateGameRequest_IsRecord_SupportsEquality()
    {
        // Arrange
        var request1 = new CreateGameRequest("TestPlayer", "secret");
        var request2 = new CreateGameRequest("TestPlayer", "secret");
        var request3 = new CreateGameRequest("TestPlayer", "different");

        // Act & Assert
        Assert.Equal(request1, request2);
        Assert.NotEqual(request1, request3);
    }

    [Fact]
    public void CreateGameRequest_ToString_ContainsPlayerName()
    {
        // Arrange
        var playerName = "TestPlayer";
        var request = new CreateGameRequest(playerName, "secret");

        // Act
        var toString = request.ToString();

        // Assert
        Assert.Contains(playerName, toString);
    }

    [Fact]
    public void CreateGameRequest_WithEmptyPlayerName_AcceptsEmptyString()
    {
        // Arrange & Act
        var request = new CreateGameRequest("", "password");

        // Assert
        Assert.Equal("", request.PlayerName);
        Assert.Equal("password", request.Password);
    }
}
