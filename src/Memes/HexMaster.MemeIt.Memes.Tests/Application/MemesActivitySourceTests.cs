using HexMaster.MemeIt.Memes.Application.Observability;
using System.Diagnostics;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public sealed class MemesActivitySourceTests
{
    [Fact]
    public void Instance_ShouldReturnActivitySource()
    {
        // Act
        var activitySource = MemesActivitySource.Instance;

        // Assert
        Assert.NotNull(activitySource);
        Assert.Equal("HexMaster.MemeIt.Memes", activitySource.Name);
    }

    [Fact]
    public void SourceName_ShouldHaveCorrectValue()
    {
        // Assert
        Assert.Equal("HexMaster.MemeIt.Memes", MemesActivitySource.SourceName);
    }

    [Fact]
    public void Instance_ShouldBeAbleToStartActivity()
    {
        // Act
        using var activity = MemesActivitySource.Instance.StartActivity("TestActivity");

        // Assert - activity may be null if no listeners are registered
        // Just verify no exception is thrown
        Assert.True(true);
    }
}
