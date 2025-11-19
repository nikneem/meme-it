using Dapr.Client;
using HexMaster.MemeIt.Games.Abstractions;
using HexMaster.MemeIt.Games.Domain;
using HexMaster.MemeIt.Games.ValueObjects;
using System.Text.Json;

namespace HexMaster.MemeIt.Games.Repositories;

public class DaprGameRepository : IGameRepository
{
    private readonly DaprClient _daprClient;
    private const string StateStoreName = "statestore";

    public DaprGameRepository(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task<Game?> GetAsync(string gameCode, CancellationToken cancellationToken = default)
    {
        try
        {
            // Redis state store handles JSON serialization automatically
            var gameData = await _daprClient.GetStateAsync<GameStateData>(
                StateStoreName,
                GetStateKey(gameCode),
                cancellationToken: cancellationToken);

            return gameData?.ToDomain();
        }
        catch (Exception)
        {
            // Handle case where state doesn't exist
            return null;
        }
    }

    public async Task SaveAsync(Game game, CancellationToken cancellationToken = default)
    {
        var gameData = GameStateData.FromDomain(game);

        // Redis state store automatically serializes the object to JSON
        await _daprClient.SaveStateAsync(
            StateStoreName,
            GetStateKey(game.GameCode),
            gameData,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string gameCode, CancellationToken cancellationToken = default)
    {
        await _daprClient.DeleteStateAsync(
            StateStoreName,
            GetStateKey(gameCode),
            cancellationToken: cancellationToken);
    }

    private static string GetStateKey(string gameCode) => $"game-{gameCode}";
}

// Data transfer object for DAPR state serialization
public class GameStateData
{
    public string GameCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<GamePlayerData> Players { get; set; } = [];
    public string? Password { get; set; }
    public string? LeaderId { get; set; }
    public GameSettings Settings { get; set; } = new();
    public Dictionary<string, bool> PlayerReadyStates { get; set; } = [];
    public Dictionary<string, PlayerMemeAssignment> PlayerMemeAssignments { get; set; } = [];
    public int CurrentRound { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static GameStateData FromDomain(Game game)
    {
        return new GameStateData
        {
            GameCode = game.GameCode,
            Status = game.Status,
            Players = game.Players.Select(p => new GamePlayerData { Id = p.Id, Name = p.Name }).ToList(),
            Password = game.Password,
            LeaderId = game.LeaderId,
            Settings = game.Settings,
            PlayerReadyStates = new Dictionary<string, bool>(game.PlayerReadyStates),
            PlayerMemeAssignments = new Dictionary<string, PlayerMemeAssignment>(game.PlayerMemeAssignments),
            CurrentRound = game.CurrentRound,
            CreatedAt = game.CreatedAt,
            UpdatedAt = game.UpdatedAt
        };
    }

    public Game ToDomain()
    {
        // Create game using private constructor via reflection
        var game = (Game)Activator.CreateInstance(typeof(Game), nonPublic: true)!;

        // Now we can set the public properties directly
        game.GameCode = GameCode;
        game.Status = Status;
        game.Players = Players.Select(p => p.ToDomain()).ToList();
        game.Password = Password;
        game.LeaderId = LeaderId;
        game.Settings = Settings;
        game.PlayerReadyStates = new Dictionary<string, bool>(PlayerReadyStates);
        game.PlayerMemeAssignments = new Dictionary<string, PlayerMemeAssignment>(PlayerMemeAssignments);
        game.CurrentRound = CurrentRound;
        game.CreatedAt = CreatedAt;
        game.UpdatedAt = UpdatedAt;

        return game;
    }
}

public class GamePlayerData
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public GamePlayer ToDomain()
    {
        // Use reflection to create GamePlayer with private constructor
        var constructor = typeof(GamePlayer).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(c => c.GetParameters().Length == 0);

        var player = (GamePlayer)constructor!.Invoke(null);

        // Now we can set the public properties directly
        player.Id = Id;
        player.Name = Name;

        return player;
    }
}
