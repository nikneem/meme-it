using System;
using System.Collections.Generic;
using System.Linq;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.ValueObjects;

namespace HexMaster.MemeIt.Games.Domains;

/// <summary>
/// Aggregate root for Meme-It games. Encapsulates lifecycle, players, and rounds.
/// </summary>
public sealed class Game : IGame
{
    private const int DefaultRoundTarget = 5;
    private const int GameCodeLength = 8;

    private readonly List<GamePlayer> _players = new();
    private readonly List<GameRound> _rounds = new();
    private readonly int _roundTarget;

    public Game(string gameCode, Guid adminPlayerId, string? password = null, IEnumerable<IGamePlayer>? initialPlayers = null, DateTimeOffset? createdAt = null, int roundTarget = DefaultRoundTarget)
    {
        GameCode = ValidateGameCode(gameCode);
        AdminPlayerId = ValidateGuid(adminPlayerId, nameof(adminPlayerId));
        Password = string.IsNullOrWhiteSpace(password) ? null : password;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        State = GameState.Lobby;
        _roundTarget = roundTarget > 0 ? roundTarget : DefaultRoundTarget;

        if (initialPlayers is not null)
        {
            foreach (var player in initialPlayers)
            {
                _players.Add(new GamePlayer(player.PlayerId, player.DisplayName, player.IsReady));
            }
        }
    }

    public string GameCode { get; }

    public string? Password { get; private set; }

    public Guid AdminPlayerId { get; }

    public IReadOnlyCollection<IGamePlayer> Players => _players.Cast<IGamePlayer>().ToArray();

    public IReadOnlyCollection<IGameRound> Rounds => _rounds.Cast<IGameRound>().ToArray();

    public GameState State { get; private set; }

    public int CurrentRound { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public int RoundTarget => _roundTarget;

    public void AddPlayer(Guid playerId, string displayName, string? passwordAttempt = null)
    {
        EnsureState(State.Equals(GameState.Lobby), "Players can only join while the game is in the lobby.");
        ValidateGuid(playerId, nameof(playerId));

        if (_players.Any(p => p.PlayerId == playerId))
        {
            throw new InvalidOperationException("Player has already joined this game.");
        }

        if (Password is not null && !string.Equals(Password, passwordAttempt, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid password provided for this game.");
        }

        _players.Add(new GamePlayer(playerId, displayName));
    }

    public void RemovePlayer(Guid playerId)
    {
        if (playerId == AdminPlayerId)
        {
            throw new InvalidOperationException("The admin player cannot be removed from the game.");
        }

        var removed = _players.RemoveAll(p => p.PlayerId == playerId) > 0;
        if (!removed)
        {
            return;
        }

        foreach (var round in _rounds)
        {
            round.RemoveSubmissionForPlayer(playerId);
        }
    }

    public void SetPlayerReady(Guid playerId, bool isReady)
    {
        EnsureState(State.Equals(GameState.Lobby), "Player ready status can only be changed in the lobby.");
        ValidateGuid(playerId, nameof(playerId));

        var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
        if (player is null)
        {
            throw new InvalidOperationException("Player is not part of this game.");
        }

        player.IsReady = isReady;
    }

    public bool AreAllPlayersReady()
    {
        if (_players.Count == 0)
        {
            return false;
        }

        return _players.All(p => p.IsReady);
    }

    public void AddMemeSubmission(int roundNumber, IMemeSubmission submission)
    {
        if (roundNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber), roundNumber, "Round numbers start at 1.");
        }

        EnsureState(
            State.Equals(GameState.InProgress) || State.Equals(GameState.Scoring),
            "Submissions can only be made while the game is running.");

        if (_players.All(p => p.PlayerId != submission.PlayerId))
        {
            throw new InvalidOperationException("Player must be part of the game to submit a meme.");
        }

        var round = _rounds.OfType<GameRound>().FirstOrDefault(r => r.RoundNumber == roundNumber)
                    ?? throw new InvalidOperationException("Round not found. Did you call NextRound()? ");

        round.UpsertSubmission(submission);
    }

    public IGameRound NextRound()
    {
        EnsureState(!State.Equals(GameState.Completed), "Cannot start new rounds after the game is completed.");

        if (_rounds.Count >= _roundTarget)
        {
            throw new InvalidOperationException("Maximum number of rounds reached.");
        }

        var nextNumber = _rounds.Count == 0 ? 1 : _rounds.Max(r => r.RoundNumber) + 1;
        var round = new GameRound(nextNumber);
        _rounds.Add(round);
        State = GameState.InProgress;
        CurrentRound = nextNumber;
        return round;
    }

    public void ChangeState(GameState targetState)
    {
        State = State.TransitionTo(targetState);
    }

    public void Finish()
    {
        State = State.TransitionTo(GameState.Completed);
    }

    public void MarkCreativePhaseEnded(int roundNumber)
    {
        var round = _rounds.OfType<GameRound>().FirstOrDefault(r => r.RoundNumber == roundNumber)
                    ?? throw new InvalidOperationException($"Round {roundNumber} not found.");

        round.MarkCreativePhaseEnded();
    }

    public void MarkScorePhaseEnded(int roundNumber)
    {
        var round = _rounds.OfType<GameRound>().FirstOrDefault(r => r.RoundNumber == roundNumber)
                    ?? throw new InvalidOperationException($"Round {roundNumber} not found.");

        round.MarkScorePhaseEnded();
    }

    public IGameRound? GetRound(int roundNumber)
    {
        return _rounds.FirstOrDefault(r => r.RoundNumber == roundNumber);
    }

    public IMemeSubmission? GetRandomUnratedSubmissionForRound(int roundNumber)
    {
        var round = _rounds.OfType<GameRound>().FirstOrDefault(r => r.RoundNumber == roundNumber);
        return round?.GetRandomUnratedSubmission();
    }

    public void AddScore(int roundNumber, Guid memeId, Guid voterId, int score)
    {
        var round = _rounds.FirstOrDefault(r => r.RoundNumber == roundNumber);
        if (round == null)
        {
            throw new InvalidOperationException($"Round {roundNumber} not found.");
        }

        round.AddScore(memeId, voterId, score);
    }

    private static string ValidateGameCode(string gameCode)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
        {
            throw new ArgumentException("Game code must be provided", nameof(gameCode));
        }

        if (gameCode.Length != GameCodeLength)
        {
            throw new ArgumentException($"Game code must be {GameCodeLength} characters long.", nameof(gameCode));
        }

        return gameCode.ToUpperInvariant();
    }

    private static Guid ValidateGuid(Guid value, string argumentName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A non-empty guid is required", argumentName);
        }

        return value;
    }

    private static void EnsureState(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
