using Microsoft.AspNetCore.Mvc;
using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;

namespace MemeIt.Api.Controllers;

/// <summary>
/// Controller for meme operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MemesController : ControllerBase
{
    private readonly IMemeLibraryService _memeLibraryService;
    private readonly ILogger<MemesController> _logger;

    public MemesController(IMemeLibraryService memeLibraryService, ILogger<MemesController> logger)
    {
        _memeLibraryService = memeLibraryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a random meme for a player with specified categories
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="categories">Comma-separated list of category IDs</param>
    /// <param name="excludedMemeIds">Comma-separated list of meme IDs to exclude</param>
    /// <returns>A random meme</returns>
    [HttpGet("random")]
    public async Task<ActionResult<Meme>> GetRandomMeme(
        [FromQuery] string playerId,
        [FromQuery] string categories,
        [FromQuery] string? excludedMemeIds = null)
    {
        try
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return BadRequest("Player ID is required");
            }

            if (string.IsNullOrEmpty(categories))
            {
                return BadRequest("At least one category is required");
            }

            var categoryList = categories.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var excludedList = string.IsNullOrEmpty(excludedMemeIds) 
                ? [] 
                : excludedMemeIds.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var meme = await _memeLibraryService.GetRandomMemeForPlayerAsync(
                playerId, 
                categoryList, 
                excludedList);

            if (meme == null)
            {
                return NotFound("No memes found matching the criteria");
            }

            return Ok(meme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random meme for player {PlayerId}", playerId);
            return StatusCode(500, "An error occurred while retrieving the meme");
        }
    }

    /// <summary>
    /// Records that a meme was used by a player
    /// </summary>
    /// <param name="memeId">The meme ID</param>
    /// <param name="playerId">The player ID</param>
    /// <returns>Success result</returns>
    [HttpPost("{memeId}/usage")]
    public async Task<ActionResult> RecordMemeUsage(string memeId, [FromQuery] string playerId)
    {
        try
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return BadRequest("Player ID is required");
            }

            await _memeLibraryService.RecordMemeUsageAsync(memeId, playerId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording meme usage for meme {MemeId} and player {PlayerId}", memeId, playerId);
            return StatusCode(500, "An error occurred while recording meme usage");
        }
    }

    /// <summary>
    /// Gets a meme by ID
    /// </summary>
    /// <param name="id">The meme ID</param>
    /// <returns>The meme</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Meme>> GetMeme(string id)
    {
        try
        {
            var meme = await _memeLibraryService.GetMemeByIdAsync(id);
            if (meme == null)
            {
                return NotFound();
            }

            return Ok(meme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting meme {MemeId}", id);
            return StatusCode(500, "An error occurred while retrieving the meme");
        }
    }

    /// <summary>
    /// Gets all memes with pagination (Note: This requires direct repository access as the service doesn't expose this method)
    /// </summary>
    /// <param name="skip">Number of memes to skip</param>
    /// <param name="take">Number of memes to take</param>
    /// <returns>List of memes</returns>
    [HttpGet]
    public ActionResult<IReadOnlyList<Meme>> GetMemes([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        try
        {
            // Note: The service interface doesn't have GetMemesAsync
            // This would require either:
            // 1. Adding the method to IMemeLibraryService
            // 2. Injecting IMemeRepository directly
            // 3. Removing this endpoint
            return NotFound("This endpoint is not implemented. Use /api/memes/random to get a random meme.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memes with skip {Skip} and take {Take}", skip, take);
            return StatusCode(500, "An error occurred while retrieving memes");
        }
    }
}
