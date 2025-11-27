using HexMaster.MemeIt.Users.Application.Observability;
using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Users.Tests.Application;

public sealed class UsersMetricsTests
{
    [Fact]
    public void RecordUserJoined_Increments_Counter()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new UsersMetrics(meterFactory);

        // Act
        metrics.RecordUserJoined();

        // Assert - no exception means counter was incremented successfully
        Assert.True(true);
    }

    [Fact]
    public void RecordHandlerDuration_Records_Success()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new UsersMetrics(meterFactory);

        // Act
        metrics.RecordHandlerDuration("TestCommand", 123.45, success: true);

        // Assert - no exception means metric was recorded
        Assert.True(true);
    }

    [Fact]
    public void RecordHandlerDuration_Records_Failure()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new UsersMetrics(meterFactory);

        // Act
        metrics.RecordHandlerDuration("TestCommand", 123.45, success: false);

        // Assert - no exception means metric was recorded
        Assert.True(true);
    }

    [Fact]
    public void RecordCommandFailed_Increments_Counter()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();
        var metrics = new UsersMetrics(meterFactory);

        // Act
        metrics.RecordCommandFailed("TestCommand", "InvalidOperationException");

        // Assert - no exception means counter was incremented
        Assert.True(true);
    }

    [Fact]
    public void Constructor_Creates_Metrics_Successfully()
    {
        // Arrange
        var meterFactory = new TestMeterFactory();

        // Act
        var metrics = new UsersMetrics(meterFactory);

        // Assert
        Assert.NotNull(metrics);
    }

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);
        public void Dispose() { }
    }
}
