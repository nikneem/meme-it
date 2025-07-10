namespace HexMaster.MemeIt.Games.ValueObjects;

public abstract class GameStatus
{
    public static readonly GameStatus Uninitialized = new GameStatusUninitialized();
    public static readonly GameStatus Waiting = new GameStatusWaiting();
    public static readonly GameStatus Active = new GameStatusActive();
    public static readonly GameStatus Finished = new GameStatusFinished();

    public static GameStatus[] All =
    [
        Uninitialized,
        Waiting,
        Active,
        Finished
    ];

    public abstract string Id { get; }
}

public sealed class GameStatusUninitialized : GameStatus
{
    public override string Id => GameStatusName.Uninitialized;
}
public sealed class GameStatusWaiting: GameStatus
{
    public override string Id => GameStatusName.Waiting;
}
public sealed class GameStatusActive : GameStatus
{
    public override string Id => GameStatusName.Active;
}

public sealed class GameStatusFinished : GameStatus
{
    public override string Id => GameStatusName.Finished;
}