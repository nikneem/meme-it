using MemeIt.Games.Abstractions.Models;
using Microsoft.AspNetCore.Mvc;
using MemeIt.Games.Abstractions.Services;

namespace MemeIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(IGameService gameService, ILogger<GamesController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        try
        {
            var (gameId, gameCode) = await _gameService.CreateGameAsync(
                request.PlayerId, 
                request.PlayerName, 
                request.GameName, 
                request.Options);

            return Ok(new { GameId = gameId, GameCode = gameCode });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating game");
            return BadRequest("Failed to create game");
        }
    }

    [HttpGet("{gameId}")]
    public async Task<IActionResult> GetGame(string gameId)
    {
        var game = await _gameService.GetGameAsync(gameId);
        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    [HttpGet("by-code/{gameCode}")]
    public async Task<IActionResult> GetGameByCode(string gameCode)
    {
        var game = await _gameService.GetGameByCodeAsync(gameCode);
        if (game == null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableGames()
    {
        var games = await _gameService.GetAvailableGamesAsync();
        return Ok(games);
    }

    [HttpPost("{gameId}/join")]
    public async Task<IActionResult> JoinGame(string gameId, [FromBody] JoinGameRequest request)
    {
        var success = await _gameService.JoinGameAsync(
            gameId, 
            request.PlayerId, 
            request.PlayerName, 
            request.Password);

        if (success)
        {
            return Ok();
        }

        return BadRequest("Failed to join game");
    }

    [HttpPost("join-by-code")]
    public async Task<IActionResult> JoinGameByCode([FromBody] JoinGameByCodeRequest request)
    {
        var success = await _gameService.JoinGameByCodeAsync(
            request.GameCode,
            request.PlayerId, 
            request.PlayerName, 
            request.Password);

        if (success)
        {
            return Ok();
        }

        return BadRequest("Failed to join game");
    }

    [HttpPost("{gameId}/leave")]
    public async Task<IActionResult> LeaveGame(string gameId, [FromBody] LeaveGameRequest request)
    {
        var success = await _gameService.LeaveGameAsync(gameId, request.PlayerId);
        if (success)
        {
            return Ok();
        }

        return BadRequest("Failed to leave game");
    }

    [HttpPost("{gameId}/start")]
    public async Task<IActionResult> StartGame(string gameId, [FromBody] StartGameRequest request)
    {
        var success = await _gameService.StartGameAsync(gameId, request.PlayerId);
        if (success)
        {
            return Ok();
        }

        return BadRequest("Failed to start game");
    }

    [HttpPost("{gameId}/submit-text")]
    public async Task<IActionResult> SubmitMemeText(string gameId, [FromBody] SubmitMemeTextRequest request)
    {
        var success = await _gameService.SubmitMemeTextAsync(
            gameId, 
            request.PlayerId, 
            request.TextEntries);

        if (success)
        {
            return Ok();
        }

        return BadRequest("Failed to submit meme text");
    }

    [HttpPost("{gameId}/submit-score")]
    public async Task<IActionResult> SubmitScore(string gameId, [FromBody] SubmitScoreRequest request)
    {
        var success = await _gameService.SubmitScoreAsync(
            gameId, 
            request.PlayerId, 
            request.TargetPlayerId, 
            request.Score);

        if (success)
        {
            return Ok();
        }

        return BadRequest("Failed to submit score");
    }

    [HttpGet("{gameId}/scores")]
    public async Task<IActionResult> GetScores(string gameId)
    {
        var scores = await _gameService.GetScoresAsync(gameId);
        return Ok(scores);
    }

    [HttpGet("{gameId}/current-round")]
    public async Task<IActionResult> GetCurrentRound(string gameId)
    {
        var round = await _gameService.GetCurrentRoundAsync(gameId);
        if (round == null)
        {
            return NotFound();
        }

        return Ok(round);
    }
}

public record CreateGameRequest(
    string PlayerId,
    string PlayerName,
    string GameName,
    GameOptions? Options = null);

public record JoinGameRequest(
    string PlayerId,
    string PlayerName,
    string? Password = null);

public record JoinGameByCodeRequest(
    string GameCode,
    string PlayerId,
    string PlayerName,
    string? Password = null);

public record LeaveGameRequest(string PlayerId);

public record StartGameRequest(string PlayerId);

public record SubmitMemeTextRequest(
    string PlayerId,
    Dictionary<string, string> TextEntries);

public record SubmitScoreRequest(
    string PlayerId,
    string TargetPlayerId,
    int Score);
