using HexMaster.MemeIt.Games.ValueObjects;
using HexMaster.MemeIt.Games;

namespace HexMaster.MemeIt.Games.Tests.ValueObjects;

public class GameStatusTests
{
    [Fact]
    public void GameStatus_Uninitialized_HasCorrectId()
    {
        // Act & Assert
        Assert.Equal(GameStatusName.Uninitialized, GameStatus.Uninitialized.Id);
    }

    [Fact]
    public void GameStatus_Waiting_HasCorrectId()
    {
        // Act & Assert
        Assert.Equal(GameStatusName.Waiting, GameStatus.Waiting.Id);
    }

    [Fact]
    public void GameStatus_Active_HasCorrectId()
    {
        // Act & Assert
        Assert.Equal(GameStatusName.Active, GameStatus.Active.Id);
    }

    [Fact]
    public void GameStatus_Finished_HasCorrectId()
    {
        // Act & Assert
        Assert.Equal(GameStatusName.Finished, GameStatus.Finished.Id);
    }

    [Fact]
    public void GameStatus_All_ContainsAllStatuses()
    {
        // Act
        var allStatuses = GameStatus.All;

        // Assert
        Assert.Equal(4, allStatuses.Length);
        Assert.Contains(GameStatus.Uninitialized, allStatuses);
        Assert.Contains(GameStatus.Waiting, allStatuses);
        Assert.Contains(GameStatus.Active, allStatuses);
        Assert.Contains(GameStatus.Finished, allStatuses);
    }

    [Fact]
    public void GameStatus_All_StatusesAreUnique()
    {
        // Act
        var allStatuses = GameStatus.All;
        var uniqueIds = allStatuses.Select(s => s.Id).Distinct().ToArray();

        // Assert
        Assert.Equal(allStatuses.Length, uniqueIds.Length);
    }

    [Fact]
    public void GameStatusUninitialized_Id_ReturnsCorrectValue()
    {
        // Arrange
        var status = new GameStatusUninitialized();

        // Act & Assert
        Assert.Equal(GameStatusName.Uninitialized, status.Id);
    }

    [Fact]
    public void GameStatusWaiting_Id_ReturnsCorrectValue()
    {
        // Arrange
        var status = new GameStatusWaiting();

        // Act & Assert
        Assert.Equal(GameStatusName.Waiting, status.Id);
    }

    [Fact]
    public void GameStatusActive_Id_ReturnsCorrectValue()
    {
        // Arrange
        var status = new GameStatusActive();

        // Act & Assert
        Assert.Equal(GameStatusName.Active, status.Id);
    }

    [Fact]
    public void GameStatusFinished_Id_ReturnsCorrectValue()
    {
        // Arrange
        var status = new GameStatusFinished();

        // Act & Assert
        Assert.Equal(GameStatusName.Finished, status.Id);
    }

    [Fact]
    public void GameStatus_StaticInstances_AreSingletons()
    {
        // Act & Assert
        Assert.Same(GameStatus.Uninitialized, GameStatus.Uninitialized);
        Assert.Same(GameStatus.Waiting, GameStatus.Waiting);
        Assert.Same(GameStatus.Active, GameStatus.Active);
        Assert.Same(GameStatus.Finished, GameStatus.Finished);
    }

    [Theory]
    [InlineData(GameStatusName.Uninitialized)]
    [InlineData(GameStatusName.Waiting)]
    [InlineData(GameStatusName.Active)]
    [InlineData(GameStatusName.Finished)]
    public void GameStatus_AllStatuses_HaveExpectedIds(string expectedId)
    {
        // Arrange
        var allIds = GameStatus.All.Select(s => s.Id).ToArray();

        // Act & Assert
        Assert.Contains(expectedId, allIds);
    }
}
