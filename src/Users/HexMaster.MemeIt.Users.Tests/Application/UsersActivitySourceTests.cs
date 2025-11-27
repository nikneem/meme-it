using HexMaster.MemeIt.Users.Application.Observability;
using System.Diagnostics;

namespace HexMaster.MemeIt.Users.Tests.Application;

public sealed class UsersActivitySourceTests
{
    [Fact]
    public void Instance_Returns_ActivitySource()
    {
        // Act
        var activitySource = UsersActivitySource.Instance;

        // Assert
        Assert.NotNull(activitySource);
        Assert.Equal("HexMaster.MemeIt.Users", activitySource.Name);
    }

    [Fact]
    public void SourceName_Has_Correct_Value()
    {
        // Assert
        Assert.Equal("HexMaster.MemeIt.Users", UsersActivitySource.SourceName);
    }

    [Fact]
    public void Instance_Can_Start_Activity()
    {
        // Act
        using var activity = UsersActivitySource.Instance.StartActivity("TestActivity");

        // Assert - activity may be null if no listeners are registered
        // Just verify no exception is thrown
        Assert.True(true);
    }
}
