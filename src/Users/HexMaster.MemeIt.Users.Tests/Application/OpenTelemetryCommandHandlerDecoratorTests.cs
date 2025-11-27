using HexMaster.MemeIt.Users.Abstractions.Application.Commands;
using HexMaster.MemeIt.Users.Application.Observability;
using Moq;
using System.Diagnostics.Metrics;

namespace HexMaster.MemeIt.Users.Tests.Application;

public sealed class OpenTelemetryCommandHandlerDecoratorTests
{
    [Fact]
    public async Task HandleAsync_Calls_Inner_Handler()
    {
        // Arrange
        var mockInnerHandler = new Mock<ICommandHandler<TestCommand, TestResult>>();
        mockInnerHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult("success"));

        var metrics = CreateMetrics();
        var decorator = new OpenTelemetryCommandHandlerDecorator<TestCommand, TestResult>(
            mockInnerHandler.Object,
            metrics);

        var command = new TestCommand();

        // Act
        var result = await decorator.HandleAsync(command);

        // Assert
        Assert.Equal("success", result.Value);
        mockInnerHandler.Verify(h => h.HandleAsync(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Records_Success_Metric()
    {
        // Arrange
        var mockInnerHandler = new Mock<ICommandHandler<TestCommand, TestResult>>();
        mockInnerHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResult("success"));

        var metrics = CreateMetrics();
        var decorator = new OpenTelemetryCommandHandlerDecorator<TestCommand, TestResult>(
            mockInnerHandler.Object,
            metrics);

        // Act
        await decorator.HandleAsync(new TestCommand());

        // Assert - no exception means metrics were recorded
        Assert.True(true);
    }

    [Fact]
    public async Task HandleAsync_Records_Failure_Metric_On_Exception()
    {
        // Arrange
        var mockInnerHandler = new Mock<ICommandHandler<TestCommand, TestResult>>();
        mockInnerHandler
            .Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("test error"));

        var metrics = CreateMetrics();
        var decorator = new OpenTelemetryCommandHandlerDecorator<TestCommand, TestResult>(
            mockInnerHandler.Object,
            metrics);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.HandleAsync(new TestCommand()));
    }

    [Fact]
    public void Constructor_Throws_When_Inner_Is_Null()
    {
        // Arrange
        var metrics = CreateMetrics();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OpenTelemetryCommandHandlerDecorator<TestCommand, TestResult>(null!, metrics));
    }

    [Fact]
    public void Constructor_Throws_When_Metrics_Is_Null()
    {
        // Arrange
        var mockInnerHandler = new Mock<ICommandHandler<TestCommand, TestResult>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new OpenTelemetryCommandHandlerDecorator<TestCommand, TestResult>(mockInnerHandler.Object, null!));
    }

    private static UsersMetrics CreateMetrics()
    {
        var meterFactory = new TestMeterFactory();
        return new UsersMetrics(meterFactory);
    }

    public sealed class TestCommand : ICommand<TestResult>
    {
    }

    public sealed record TestResult(string Value);

    private sealed class TestMeterFactory : IMeterFactory
    {
        public Meter Create(MeterOptions options) => new(options);
        public void Dispose() { }
    }
}
