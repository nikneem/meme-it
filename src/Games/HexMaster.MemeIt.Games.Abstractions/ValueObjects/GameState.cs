namespace HexMaster.MemeIt.Games.Abstractions.ValueObjects;

/// <summary>
/// Value object that enforces the valid lifecycle transitions for a game.
/// </summary>
public sealed class GameState : IEquatable<GameState>
{
    private readonly IReadOnlyCollection<string> _allowedTransitions;

    private GameState(string name, int order, params string[] allowedTransitions)
    {
        Name = name;
        Order = order;
        _allowedTransitions = allowedTransitions;
    }

    public string Name { get; }

    public int Order { get; }

    public static GameState Lobby { get; } = new(nameof(Lobby), 0, nameof(InProgress));

    public static GameState InProgress { get; } = new(nameof(InProgress), 1, nameof(Scoring), nameof(Completed));

    public static GameState Scoring { get; } = new(nameof(Scoring), 2, nameof(InProgress), nameof(Completed));

    public static GameState Completed { get; } = new(nameof(Completed), 3);

    private static readonly IReadOnlyDictionary<string, GameState> KnownStates =
        new Dictionary<string, GameState>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Lobby)] = Lobby,
            [nameof(InProgress)] = InProgress,
            [nameof(Scoring)] = Scoring,
            [nameof(Completed)] = Completed
        };

    public bool CanTransitionTo(GameState target)
        => _allowedTransitions.Contains(target.Name);

    public GameState TransitionTo(GameState target)
    {
        if (!CanTransitionTo(target))
        {
            throw new InvalidOperationException($"Cannot transition from {Name} to {target.Name}.");
        }

        return target;
    }

    public static GameState FromName(string name)
    {
        if (!KnownStates.TryGetValue(name, out var state))
        {
            throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown game state.");
        }

        return state;
    }

    public bool Equals(GameState? other)
        => other is not null && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj)
        => obj is GameState other && Equals(other);

    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    public override string ToString() => Name;
}
