using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;
using Microsoft.Extensions.Logging;

namespace MemeIt.Library.Examples;

/// <summary>
/// Example service demonstrating how to use the MemeIt.Library
/// </summary>
public class MemeGameService
{
    private readonly IMemeLibraryService _memeLibraryService;
    private readonly ILogger<MemeGameService> _logger;

    public MemeGameService(IMemeLibraryService memeLibraryService, ILogger<MemeGameService> logger)
    {
        _memeLibraryService = memeLibraryService ?? throw new ArgumentNullException(nameof(memeLibraryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Starts a new meme game for a player
    /// </summary>
    /// <param name="playerId">The unique identifier for the player</param>
    /// <param name="preferredCategories">Optional list of preferred meme categories</param>
    /// <param name="difficultyLevel">Difficulty level (1-5)</param>
    /// <returns>A game session with memes</returns>
    public async Task<MemeGameSession> StartGameAsync(
        string playerId, 
        IReadOnlyList<string>? preferredCategories = null,
        int difficultyLevel = 1)
    {
        _logger.LogInformation("Starting game for player {PlayerId} with difficulty {Difficulty}", playerId, difficultyLevel);

        try
        {
            // Get available categories if none specified
            var categories = preferredCategories?.Count > 0 
                ? preferredCategories 
                : (await _memeLibraryService.GetAvailableCategoriesAsync())
                    .Take(3) // Limit to 3 random categories for variety
                    .Select(c => c.Id)
                    .ToList();

            // Validate categories
            var validCategories = await _memeLibraryService.ValidateCategoriesAsync(categories);
            if (validCategories.Count == 0)
            {
                throw new InvalidOperationException("No valid categories available for game");
            }

            // Create game session with multiple memes
            var gameSession = new MemeGameSession
            {
                SessionId = Guid.NewGuid().ToString(),
                PlayerId = playerId,
                Categories = validCategories,
                DifficultyLevel = difficultyLevel,
                StartTime = DateTimeOffset.UtcNow,
                Memes = new List<Meme>()
            };

            // Get initial set of memes for the game
            var memesToGet = Math.Max(3, difficultyLevel * 2); // More memes for higher difficulty
            var excludeIds = new List<string>();

            for (int i = 0; i < memesToGet; i++)
            {
                var meme = await _memeLibraryService.GetRandomMemeForPlayerAsync(
                    playerId, 
                    validCategories, 
                    excludeIds);

                if (meme != null)
                {
                    gameSession.Memes.Add(meme);
                    excludeIds.Add(meme.Id);

                    // Record usage
                    await _memeLibraryService.RecordMemeUsageAsync(meme.Id, playerId);
                }
            }

            _logger.LogInformation("Game session {SessionId} created with {MemeCount} memes for player {PlayerId}", 
                gameSession.SessionId, gameSession.Memes.Count, playerId);

            return gameSession;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game for player {PlayerId}", playerId);
            throw;
        }
    }

    /// <summary>
    /// Gets the next meme for an ongoing game session
    /// </summary>
    /// <param name="sessionId">The game session identifier</param>
    /// <param name="playerId">The player identifier</param>
    /// <param name="usedMemeIds">List of memes already used in this session</param>
    /// <returns>Next meme or null if no more available</returns>
    public async Task<Meme?> GetNextMemeAsync(string sessionId, string playerId, IReadOnlyList<string> usedMemeIds)
    {
        _logger.LogDebug("Getting next meme for session {SessionId}, player {PlayerId}", sessionId, playerId);

        try
        {
            // For this example, we'll use all categories
            var categories = (await _memeLibraryService.GetAvailableCategoriesAsync())
                .Select(c => c.Id)
                .ToList();

            var meme = await _memeLibraryService.GetRandomMemeForPlayerAsync(playerId, categories, usedMemeIds);

            if (meme != null)
            {
                await _memeLibraryService.RecordMemeUsageAsync(meme.Id, playerId);
                _logger.LogDebug("Selected meme {MemeId} for session {SessionId}", meme.Id, sessionId);
            }
            else
            {
                _logger.LogWarning("No more memes available for session {SessionId}", sessionId);
            }

            return meme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get next meme for session {SessionId}", sessionId);
            throw;
        }
    }

    /// <summary>
    /// Gets popular memes for trending display
    /// </summary>
    /// <param name="count">Number of memes to return</param>
    /// <returns>List of popular memes</returns>
    public async Task<IReadOnlyList<Meme>> GetTrendingMemesAsync(int count = 10)
    {
        _logger.LogDebug("Getting {Count} trending memes", count);

        try
        {
            // Get all active categories
            var categories = (await _memeLibraryService.GetAvailableCategoriesAsync())
                .Select(c => c.Id)
                .ToList();

            // For this example, we'll get memes from all categories
            // In a real implementation, you might want to add a method to get memes sorted by popularity
            var trendingMemes = new List<Meme>();
            var excludeIds = new List<string>();

            // Get multiple memes to simulate trending
            for (int i = 0; i < count && trendingMemes.Count < count; i++)
            {
                var meme = await _memeLibraryService.GetRandomMemeForPlayerAsync(
                    "trending-user", 
                    categories, 
                    excludeIds);

                if (meme != null)
                {
                    trendingMemes.Add(meme);
                    excludeIds.Add(meme.Id);
                }
            }

            _logger.LogInformation("Retrieved {Count} trending memes", trendingMemes.Count);
            return trendingMemes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get trending memes");
            throw;
        }
    }

    ///// <summary>
    ///// Creates sample memes and categories for testing/demo purposes
    ///// </summary>
    ///// <returns>List of created memes</returns>
    //public async Task<IReadOnlyList<Meme>> CreateSampleDataAsync()
    //{
    //    _logger.LogInformation("Creating sample memes and categories");

    //    // Note: This would require additional repository methods in a real implementation
    //    // For now, this serves as an example of how you might structure data creation

    //    var sampleMemes = new List<Meme>
    //    {
    //        new Meme
    //        {
    //            Id = Guid.NewGuid().ToString(),
    //            Name = "Distracted Boyfriend",
    //            ImageUrl = "https://example.com/distracted-boyfriend.jpg",
    //            Categories = new[] { "humor", "reactions" },
    //            TextAreas = new[]
    //            {
    //                new TextArea
    //                {
    //                    Id = "boyfriend-text",
    //                    X = 50,
    //                    Y = 20,
    //                    Width = 300,
    //                    Height = 60,
    //                    FontSize = 32,
    //                    Alignment = TextAlignment.Center
    //                }
    //            },
    //            Width = 800,
    //            Height = 450,
    //            Tags = new[] { "boyfriend", "distracted", "choice" },
    //            DifficultyLevel = 2,
    //            IsActive = true
    //        },
    //        new Meme
    //        {
    //            Id = Guid.NewGuid().ToString(),
    //            Name = "Drake Pointing",
    //            ImageUrl = "https://example.com/drake-pointing.jpg",
    //            Categories = new[] { "humor", "preferences" },
    //            TextAreas = new[]
    //            {
    //                new TextArea
    //                {
    //                    Id = "top-text",
    //                    X = 400,
    //                    Y = 100,
    //                    Width = 350,
    //                    Height = 80,
    //                    FontSize = 28,
    //                    Alignment = TextAlignment.Left
    //                },
    //                new TextArea
    //                {
    //                    Id = "bottom-text",
    //                    X = 400,
    //                    Y = 300,
    //                    Width = 350,
    //                    Height = 80,
    //                    FontSize = 28,
    //                    Alignment = TextAlignment.Left
    //                }
    //            },
    //            Width = 750,
    //            Height = 400,
    //            Tags = new[] { "drake", "preference", "choice" },
    //            DifficultyLevel = 1,
    //            IsActive = true
    //        }
    //    };

    //    _logger.LogInformation("Created {Count} sample memes", sampleMemes.Count);
    //    return sampleMemes;
    //}
}

/// <summary>
/// Represents a meme game session
/// </summary>
public class MemeGameSession
{
    public required string SessionId { get; set; }
    public required string PlayerId { get; set; }
    public required IReadOnlyList<string> Categories { get; set; }
    public required int DifficultyLevel { get; set; }
    public required DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public required List<Meme> Memes { get; set; }
    public int Score { get; set; }
    public bool IsCompleted => EndTime.HasValue;
}
