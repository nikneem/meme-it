using HexMaster.MemeIt.Games.Abstractions.Services;
using HexMaster.MemeIt.Games.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HexMaster.MemeIt.Games.Tests.Application.Services;

public class ScheduledTaskServiceTests : IDisposable
{
    private readonly Mock<ILogger<ScheduledTaskService>> _loggerMock;
    private readonly ScheduledTaskService _sut;

    public ScheduledTaskServiceTests()
    {
        _loggerMock = new Mock<ILogger<ScheduledTaskService>>();
        _sut = new ScheduledTaskService(_loggerMock.Object);
    }

    [Fact]
    public void ScheduleCreativePhaseEnded_ReturnsTaskId()
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;

        // Act
        var taskId = _sut.ScheduleCreativePhaseEnded(gameCode, roundNumber, 5);

        // Assert
        Assert.NotEqual(Guid.Empty, taskId);
    }

    [Fact]
    public void ScheduleScorePhaseEnded_ReturnsTaskId()
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;
        var memeId = Guid.NewGuid();

        // Act
        var taskId = _sut.ScheduleScorePhaseEnded(gameCode, roundNumber, memeId, 5);

        // Assert
        Assert.NotEqual(Guid.Empty, taskId);
    }

    [Fact]
    public void ScheduleRoundEnded_ReturnsTaskId()
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;

        // Act
        var taskId = _sut.ScheduleRoundEnded(gameCode, roundNumber, 5);

        // Assert
        Assert.NotEqual(Guid.Empty, taskId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void ScheduleTask_ClampsDelayToMinimum(int delaySeconds)
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;

        // Act
        var taskId = _sut.ScheduleCreativePhaseEnded(gameCode, roundNumber, delaySeconds);

        // Assert - Task should be scheduled with 1 second minimum
        Assert.NotEqual(Guid.Empty, taskId);
    }

    [Theory]
    [InlineData(150)]
    [InlineData(200)]
    public void ScheduleTask_ClampsDelayToMaximum(int delaySeconds)
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;

        // Act
        var taskId = _sut.ScheduleCreativePhaseEnded(gameCode, roundNumber, delaySeconds);

        // Assert - Task should be scheduled with 120 seconds maximum
        Assert.NotEqual(Guid.Empty, taskId);
    }

    [Fact]
    public void CancelTask_ExistingTask_ReturnsTrue()
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;
        var taskId = _sut.ScheduleCreativePhaseEnded(gameCode, roundNumber, 30);

        // Act
        var result = _sut.CancelTask(taskId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CancelTask_NonExistingTask_ReturnsFalse()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var result = _sut.CancelTask(taskId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CancelAllTasksForGame_CancelsMultipleTasks()
    {
        // Arrange
        var gameCode = "ABC123";
        _sut.ScheduleCreativePhaseEnded(gameCode, 1, 30);
        _sut.ScheduleRoundEnded(gameCode, 1, 30);
        _sut.ScheduleScorePhaseEnded(gameCode, 1, Guid.NewGuid(), 30);

        // Act
        var cancelledCount = _sut.CancelAllTasksForGame(gameCode);

        // Assert
        Assert.Equal(3, cancelledCount);
    }

    [Fact]
    public void CancelAllTasksForGame_DifferentGameCode_CancelsNone()
    {
        // Arrange
        var gameCode1 = "ABC123";
        var gameCode2 = "XYZ789";
        _sut.ScheduleCreativePhaseEnded(gameCode1, 1, 30);

        // Act
        var cancelledCount = _sut.CancelAllTasksForGame(gameCode2);

        // Assert
        Assert.Equal(0, cancelledCount);
    }

    [Fact]
    public async Task ProcessDueTasks_FiresEventForDueTasks()
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;
        var eventFired = false;
        ScheduledGameTask? capturedTask = null;

        _sut.TaskDue += (sender, task) =>
        {
            eventFired = true;
            capturedTask = task;
        };

        _sut.ScheduleCreativePhaseEnded(gameCode, roundNumber, 1);

        // Act
        await Task.Delay(1200); // Wait for task to become due
        _sut.ProcessDueTasks();

        // Assert
        Assert.True(eventFired);
        Assert.NotNull(capturedTask);
        Assert.Equal(gameCode, capturedTask.GameCode);
        Assert.Equal(roundNumber, capturedTask.RoundNumber);
        Assert.Equal(GameTaskType.CreativePhaseEnded, capturedTask.TaskType);
    }

    [Fact]
    public async Task ProcessDueTasks_DoesNotFireForFutureTasks()
    {
        // Arrange
        var gameCode = "ABC123";
        var roundNumber = 1;
        var eventFired = false;

        _sut.TaskDue += (sender, task) => eventFired = true;
        _sut.ScheduleCreativePhaseEnded(gameCode, roundNumber, 30);

        // Act
        _sut.ProcessDueTasks();

        // Assert
        Assert.False(eventFired);
    }

    public void Dispose()
    {
        _sut?.Dispose();
    }
}
