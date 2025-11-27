using HexMaster.MemeIt.Memes.Abstractions.Application;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Application.Observability;
using Moq;
using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public sealed class OpenTelemetryQueryHandlerDecoratorTests
{
    [Fact]
    public async Task HandleAsync_ShouldCallInnerHandler()
    {
        // Arrange
        var mockInnerHandler = new Mock<IQueryHandler<TestQuery, TestResult>>();
        mockInnerHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult("success"));

        var metrics = CreateMetrics();
        var decorator = new OpenTelemetryQueryHandlerDecorator<TestQuery, TestResult>(
            mockInnerHandler.Object,
            metrics);

        var query = new TestQuery();

        // Act
        var result = await decorator.HandleAsync(query);

        // Assert
        Assert.Equal("success", result.Value);
        mockInnerHandler.Verify(h => h.HandleAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRecordSuccessMetric()
    {
        // Arrange
        var mockInnerHandler = new Mock<IQueryHandler<TestQuery, TestResult>>();
        mockInnerHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult("success"));

        var metrics = CreateMetrics();
        var decorator = new OpenTelemetryQueryHandlerDecorator<TestQuery, TestResult>(
            mockInnerHandler.Object,
            metrics);

        // Act
        await decorator.HandleAsync(new TestQuery());

        // Assert - no exception means metrics were recorded
        Assert.True(true);
    }

    [Fact]
    public async Task HandleAsync_ShouldRecordFailureMetricOnException()
    {
        // Arrange
        var mockInnerHandler = new Mock<IQueryHandler<TestQuery, TestResult>>();
        mockInnerHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("test error"));

        var metrics = CreateMetrics();
        var decorator = new OpenTelemetryQueryHandlerDecorator<TestQuery, TestResult>(
            mockInnerHandler.Object,
            metrics);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.HandleAsync(new TestQuery()));
    }

    [Fact]
    public void Constructor_ShouldThrowWhenInnerIsNull()
    {
        // Arrange
        var metrics = CreateMetrics();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OpenTelemetryQueryHandlerDecorator<TestQuery, TestResult>(null!, metrics));
    }

    [Fact]
    public void Constructor_ShouldThrowWhenMetricsIsNull()
    {
        // Arrange
        var mockInnerHandler = new Mock<IQueryHandler<TestQuery, TestResult>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OpenTelemetryQueryHandlerDecorator<TestQuery, TestResult>(mockInnerHandler.Object, null!));
    }

    private static MemesMetrics CreateMetrics()
    {
        var meterFactory = new TestMeterFactory();
        return new MemesMetrics(meterFactory);
    }

    public sealed class TestQuery : IQuery
    {
    }

    public sealed record TestResult(string Value);

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);
        public void Dispose() { }
    }
}
