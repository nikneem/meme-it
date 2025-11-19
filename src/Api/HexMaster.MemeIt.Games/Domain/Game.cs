using HexMaster.MemeIt.Games.ValueObjects;

namespace HexMaster.MemeIt.Games.Domain;

public class Game
{
    public string GameCode { get; set; }
    public string Status { get; set; }
    public List<GamePlayer> Players { get; set; }
    public string? Password { get; set; }
    public string? LeaderId { get; set; }
    public GameSettings Settings { get; set; }
    public Dictionary<string, bool> PlayerReadyStates { get; set; }
    public Dictionary<string, PlayerMemeAssignment> PlayerMemeAssignments { get; set; }
    public int CurrentRound { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Private constructor for EF Core
    private Game()
    {
        GameCode = string.Empty;
        Status = GameStatus.Uninitialized.Id;
        Players = [];
        Settings = new GameSettings();
        PlayerReadyStates = [];
        PlayerMemeAssignments = [];
        CurrentRound = 1;
    }

    public static Game Create(string gameCode, string playerName, string? password = null)
    {
        var game = new Game();
        var initialPlayer = new GamePlayer(Guid.NewGuid().ToString(), playerName);
        
        game.GameCode = gameCode;
        game.Status = GameStatus.Waiting.Id;
        game.Players = [initialPlayer];
        game.Password = password;
        game.LeaderId = initialPlayer.Id;
        game.Settings = new GameSettings();
        game.PlayerReadyStates = new Dictionary<string, bool> { { initialPlayer.Id, false } };
        game.PlayerMemeAssignments = [];
        game.CurrentRound = 1;
        game.CreatedAt = DateTime.UtcNow;
        game.UpdatedAt = DateTime.UtcNow;
        
        return game;
    }

    public void JoinPlayer(string playerName, string? password = null)
    {
        if (Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Cannot join a game that is not in waiting status.");
        }
        
        if (!string.IsNullOrWhiteSpace(Password) && !Equals(Password, password))
        {
            throw new ArgumentException("The provided password does not match the game's password.");
        }
        
        if (Players.Any(p => p.Name == playerName))
        {
            throw new InvalidOperationException("A player with this name already joined.");
        }

        var newPlayer = new GamePlayer(Guid.NewGuid().ToString(), playerName);
        Players.Add(newPlayer);
        PlayerReadyStates[newPlayer.Id] = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePlayer(string playerId)
    {
        if (Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Cannot leave a game that is not in waiting status.");
        }
        
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }
        
        Players.Remove(player);
        PlayerReadyStates.Remove(playerId);
        
        // If leader leaves, assign new leader if any players remain
        if (LeaderId == playerId)
        {
            LeaderId = Players.FirstOrDefault()?.Id;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void KickPlayer(string hostPlayerId, string targetPlayerId)
    {
        if (Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Cannot kick players when the game is not in waiting status.");
        }
        
        // Verify that the host is the leader
        if (LeaderId != hostPlayerId)
        {
            throw new UnauthorizedAccessException("Only the game host can kick players.");
        }
        
        // Cannot kick yourself
        if (hostPlayerId == targetPlayerId)
        {
            throw new InvalidOperationException("Cannot kick yourself from the game.");
        }
        
        var targetPlayer = Players.FirstOrDefault(p => p.Id == targetPlayerId);
        if (targetPlayer == null)
        {
            throw new InvalidOperationException("Target player not found in this game.");
        }
        
        // Remove the player
        Players.Remove(targetPlayer);
        PlayerReadyStates.Remove(targetPlayerId);
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(string playerId, GameSettings settings)
    {
        if (LeaderId != playerId)
        {
            throw new UnauthorizedAccessException("Only the game leader can update settings.");
        }
        
        if (Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Settings can only be changed before the game has started.");
        }
        
        // Enforce limits
        if (settings.MaxPlayers < 2 || settings.MaxPlayers > 25)
            settings.MaxPlayers = Math.Clamp(settings.MaxPlayers, 2, 25);
        if (settings.NumberOfRounds < 1 || settings.NumberOfRounds > 10)
            settings.NumberOfRounds = Math.Clamp(settings.NumberOfRounds, 1, 10);
        if (string.IsNullOrWhiteSpace(settings.Category))
            settings.Category = "All";
            
        Settings = settings;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartGame(string playerId)
    {
        if (LeaderId != playerId)
        {
            throw new UnauthorizedAccessException("Only the game leader can start the game.");
        }
        
        if (Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Game can only be started from waiting status.");
        }
        
        // Check that all players are ready
        var allPlayersReady = Players.All(p => PlayerReadyStates.GetValueOrDefault(p.Id, false));
        if (!allPlayersReady)
        {
            throw new InvalidOperationException("All players must be ready before starting the game.");
        }
        
        // Require at least 2 players
        if (Players.Count < 2)
        {
            throw new InvalidOperationException("At least 2 players are required to start the game.");
        }
        
        Status = GameStatus.Active.Id;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPlayerReadyStatus(string playerId, bool isReady)
    {
        if (Status != GameStatus.Waiting.Id)
        {
            throw new InvalidOperationException("Player ready status can only be changed while the game is waiting.");
        }
        
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }

        PlayerReadyStates[playerId] = isReady;
        UpdatedAt = DateTime.UtcNow;
    }

    public PlayerMemeAssignment? GetPlayerMemeAssignment(string playerId)
    {
        // Validate player exists in the game
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }

        // Check if player has an assignment for the current round
        var key = $"{playerId}_Round{CurrentRound}";
        return PlayerMemeAssignments.GetValueOrDefault(key);
    }

    public void AssignMemeToPlayer(string playerId, string memeTemplateId, string memeTemplateName, string memeTemplateImageUrl)
    {
        // Validate player exists in the game
        var player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            throw new InvalidOperationException("Player not found in this game.");
        }

        // Only allow this when game is active/in progress
        if (Status != GameStatus.Active.Id && Status != "InProgress")
        {
            throw new InvalidOperationException("Memes can only be assigned when the game is active.");
        }

        // Create the assignment key (player + round)
        var key = $"{playerId}_Round{CurrentRound}";
        
        // Create or update the assignment
        PlayerMemeAssignments[key] = new PlayerMemeAssignment
        {
            MemeTemplateId = memeTemplateId,
            MemeTemplateName = memeTemplateName,
            MemeTemplateImageUrl = memeTemplateImageUrl,
            CurrentRound = CurrentRound,
            AssignedAt = DateTime.UtcNow
        };

        UpdatedAt = DateTime.UtcNow;
    }

    public GameState ToGameState()
    {
        return new GameState
        {
            GameCode = GameCode,
            Status = Status,
            Players = Players.Select(p => (p.Id, p.Name)).ToList(),
            Password = Password,
            LeaderId = LeaderId,
            Settings = Settings,
            PlayerReadyStates = new Dictionary<string, bool>(PlayerReadyStates),
            PlayerMemeAssignments = new Dictionary<string, PlayerMemeAssignment>(PlayerMemeAssignments),
            CurrentRound = CurrentRound
        };
    }
}

public class GamePlayer
{
    public string Id { get; set; }
    public string Name { get; set; }

    public GamePlayer(string id, string name)
    {
        Id = id;
        Name = name;
    }

    // Private constructor for serialization
    private GamePlayer()
    {
        Id = string.Empty;
        Name = string.Empty;
    }
}
