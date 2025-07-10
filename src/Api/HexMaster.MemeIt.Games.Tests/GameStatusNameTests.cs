using HexMaster.MemeIt.Games;

namespace HexMaster.MemeIt.Games.Tests;

public class GameStatusNameTests
{
    [Fact]
    public void GameStatusName_Uninitialized_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Uninitialized", GameStatusName.Uninitialized);
    }

    [Fact]
    public void GameStatusName_Waiting_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Waiting", GameStatusName.Waiting);
    }

    [Fact]
    public void GameStatusName_Active_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Active", GameStatusName.Active);
    }

    [Fact]
    public void GameStatusName_Finished_HasCorrectValue()
    {
        // Act & Assert
        Assert.Equal("Finished", GameStatusName.Finished);
    }

    [Fact]
    public void GameStatusName_AllValues_AreUnique()
    {
        // Arrange
        var allValues = new[]
        {
            GameStatusName.Uninitialized,
            GameStatusName.Waiting,
            GameStatusName.Active,
            GameStatusName.Finished
        };

        // Act
        var uniqueValues = allValues.Distinct().ToArray();

        // Assert
        Assert.Equal(allValues.Length, uniqueValues.Length);
    }

    [Fact]
    public void GameStatusName_AllValues_AreNotNullOrEmpty()
    {
        // Arrange
        var allValues = new[]
        {
            GameStatusName.Uninitialized,
            GameStatusName.Waiting,
            GameStatusName.Active,
            GameStatusName.Finished
        };

        // Act & Assert
        foreach (var value in allValues)
        {
            Assert.False(string.IsNullOrEmpty(value));
            Assert.False(string.IsNullOrWhiteSpace(value));
        }
    }

    [Theory]
    [InlineData(GameStatusName.Uninitialized)]
    [InlineData(GameStatusName.Waiting)]
    [InlineData(GameStatusName.Active)]
    [InlineData(GameStatusName.Finished)]
    public void GameStatusName_Values_AreNonEmpty(string statusName)
    {
        // Act & Assert
        Assert.NotNull(statusName);
        Assert.NotEmpty(statusName);
    }

    [Fact]
    public void GameStatusName_Values_FollowPascalCaseConvention()
    {
        // Arrange
        var allValues = new[]
        {
            GameStatusName.Uninitialized,
            GameStatusName.Waiting,
            GameStatusName.Active,
            GameStatusName.Finished
        };

        // Act & Assert
        foreach (var value in allValues)
        {
            Assert.True(char.IsUpper(value[0]), $"Status name '{value}' should start with uppercase letter");
            Assert.False(value.Contains(" "), $"Status name '{value}' should not contain spaces");
        }
    }
}
