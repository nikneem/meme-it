# Scheduled Task Service

## Overview

The `ScheduledTaskService` provides a heap-based, in-memory task scheduler for the Games API. It efficiently schedules and executes game-related tasks with precise timing using a min-heap priority queue.

## Key Features

- **Zero-overhead idle**: No periodic polling when no tasks are scheduled
- **Precise execution**: Uses dynamic timer that fires exactly when next task is due
- **Game isolation**: Multiple concurrent games can schedule independent tasks
- **Configurable delays**: 1-120 seconds (default 30s), automatically clamped to valid range
- **Cancellation support**: Cancel individual tasks or all tasks for a game
- **Thread-safe**: All operations protected with locking
- **Event-driven**: Emits events when tasks become due for external handling

## Task Types

### 1. Creative Phase Ended
Triggered when the creative phase (meme creation) completes.

**Data Available:**
- Game Code
- Round Number

**Usage:**
```csharp
var taskId = schedulingService.ScheduleCreativePhaseEnded("ABC123", roundNumber: 1, delaySeconds: 30);
```

### 2. Score Phase Ended
Triggered when scoring for a specific meme completes.

**Data Available:**
- Game Code
- Round Number
- Meme ID (Guid)

**Usage:**
```csharp
var taskId = schedulingService.ScheduleScorePhaseEnded("ABC123", roundNumber: 1, memeId, delaySeconds: 30);
```

### 3. Round Ended
Triggered when a complete round (all scoring) finishes.

**Data Available:**
- Game Code
- Round Number

**Usage:**
```csharp
var taskId = schedulingService.ScheduleRoundEnded("ABC123", roundNumber: 1, delaySeconds: 30);
```

## Architecture

### Components

1. **IScheduledTaskService** (Abstractions)
   - Public interface for scheduling and cancellation
   - Located in `HexMaster.MemeIt.Games.Abstractions.Services`

2. **ScheduledTaskService** (Core Implementation)
   - Min-heap (SortedSet) ordered by ExecuteAt timestamp
   - Dictionary lookup for O(1) cancellation by ID
   - Single dynamic timer armed to next due task
   - Emits `TaskDue` event when tasks execute
   - Located in `HexMaster.MemeIt.Games.Application.Services`

3. **ScheduledTaskWorker** (Background Service)
   - Subscribes to `TaskDue` events
   - Routes to appropriate handlers based on task type
   - TODO: Integrate with CQRS commands or integration events
   - Located in `HexMaster.MemeIt.Games.Application.Services`

### Execution Flow

```
Schedule Task → Add to Heap → Arm Timer
                                  ↓
                            Timer Fires
                                  ↓
                          Process Due Tasks
                                  ↓
                          Emit TaskDue Event
                                  ↓
                        ScheduledTaskWorker
                                  ↓
                    Execute Domain Logic / Publish Events
```

## API Endpoints

### Schedule Creative Phase Ended
```http
POST /api/games/{gameCode}/scheduling/creative-phase
Content-Type: application/json

{
  "roundNumber": 1,
  "delaySeconds": 30
}
```

### Schedule Score Phase Ended
```http
POST /api/games/{gameCode}/scheduling/score-phase
Content-Type: application/json

{
  "roundNumber": 1,
  "memeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "delaySeconds": 30
}
```

### Schedule Round Ended
```http
POST /api/games/{gameCode}/scheduling/round-ended
Content-Type: application/json

{
  "roundNumber": 1,
  "delaySeconds": 30
}
```

### Cancel Task
```http
DELETE /api/games/{gameCode}/scheduling/tasks/{taskId}
```

### Cancel All Tasks for Game
```http
DELETE /api/games/{gameCode}/scheduling/all
```

## Configuration

Registered in DI via `AddScheduledTaskService()` extension:

```csharp
builder.Services.AddScheduledTaskService();
```

This registers:
- `ScheduledTaskService` as singleton
- `IScheduledTaskService` interface
- `ScheduledTaskWorker` as hosted service

## Delay Constraints

- **Minimum**: 1 second
- **Maximum**: 120 seconds
- **Default**: 30 seconds
- Values outside range are automatically clamped

## Thread Safety

All public methods use internal locking to ensure thread-safe access to the heap and lookup dictionary. Multiple threads can safely schedule and cancel tasks concurrently.

## Testing

Unit tests verify:
- Task scheduling returns valid IDs
- Delay clamping (min/max enforcement)
- Task cancellation (individual and bulk)
- Event firing for due tasks
- No premature execution of future tasks

Run tests:
```powershell
dotnet test --filter "FullyQualifiedName~ScheduledTaskServiceTests"
```

## Future Enhancements

1. **Persistence**: Store tasks in database for restart survival
2. **Scale-out**: DB-based locking for multi-instance deployments
3. **Retry Logic**: Exponential backoff for failed task executions
4. **Metrics**: Execution latency, queue depth, failure rates
5. **CQRS Integration**: Replace TODO comments with actual command/event dispatch

## Implementation Notes

- Uses `SortedSet<T>` with custom comparer for O(log n) heap operations
- Timer disposal/re-arming handled automatically
- No busy-wait or periodic polling
- Worker subscribes via event pattern for loose coupling
- Follows ADR 0002 (Modular Monolith), ADR 0004 (CQRS), ADR 0005 (Minimal APIs)
