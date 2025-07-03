using Microsoft.Extensions.Logging;
using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;

namespace MemeIt.Library.Services;

/// <summary>
/// Main service for meme library operations
/// </summary>
public class MemeLibraryService : IMemeLibraryService
{
    private readonly IMemeRepository _memeRepository;
    private readonly IMemeCategoryRepository _categoryRepository;
    private readonly ILogger<MemeLibraryService> _logger;

    public MemeLibraryService(
        IMemeRepository memeRepository,
        IMemeCategoryRepository categoryRepository,
        ILogger<MemeLibraryService> logger)
    {
        _memeRepository = memeRepository ?? throw new ArgumentNullException(nameof(memeRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Meme?> GetRandomMemeForPlayerAsync(
        string playerId,
        IReadOnlyList<string> categories,
        IReadOnlyList<string> excludedMemeIds = null!,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
        }

        _logger.LogDebug("Getting random meme for player: {PlayerId}, categories: {Categories}", 
            playerId, string.Join(", ", categories ?? []));

        try
        {
            // If no categories specified, get all active categories
            var categoriesToUse = categories?.Count > 0 ? categories : await GetAllActiveCategoryIdsAsync(cancellationToken);

            if (categoriesToUse.Count == 0)
            {
                _logger.LogWarning("No categories available for meme selection for player: {PlayerId}", playerId);
                return null;
            }

            // Validate categories exist and are active
            var validCategories = await ValidateCategoriesAsync(categoriesToUse, cancellationToken);
            if (validCategories.Count == 0)
            {
                _logger.LogWarning("No valid categories found for player: {PlayerId}", playerId);
                return null;
            }

            var meme = await _memeRepository.GetRandomMemeAsync(validCategories, excludedMemeIds ?? [], cancellationToken);
            
            if (meme != null)
            {
                _logger.LogInformation("Selected meme {MemeId} for player {PlayerId}", meme.Id, playerId);
            }
            else
            {
                _logger.LogWarning("No meme found for player {PlayerId} with categories: {Categories}", 
                    playerId, string.Join(", ", validCategories));
            }

            return meme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random meme for player: {PlayerId}", playerId);
            throw;
        }
    }

    public async Task<Meme?> GetMemeByIdAsync(string memeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(memeId))
        {
            throw new ArgumentException("Meme ID cannot be null or empty", nameof(memeId));
        }

        _logger.LogDebug("Getting meme by ID: {MemeId}", memeId);

        try
        {
            var meme = await _memeRepository.GetMemeByIdAsync(memeId, cancellationToken);
            
            if (meme == null)
            {
                _logger.LogWarning("Meme not found: {MemeId}", memeId);
            }
            else
            {
                _logger.LogDebug("Found meme: {MemeId} - {MemeName}", meme.Id, meme.Name);
            }

            return meme;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting meme by ID: {MemeId}", memeId);
            throw;
        }
    }

    public async Task<IReadOnlyList<MemeCategory>> GetAvailableCategoriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all available categories");

        try
        {
            var categories = await _categoryRepository.GetActiveCategoriesAsync(cancellationToken);
            
            _logger.LogDebug("Found {CategoryCount} available categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available categories");
            throw;
        }
    }

    public async Task RecordMemeUsageAsync(string memeId, string playerId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(memeId))
        {
            throw new ArgumentException("Meme ID cannot be null or empty", nameof(memeId));
        }

        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw new ArgumentException("Player ID cannot be null or empty", nameof(playerId));
        }

        _logger.LogDebug("Recording meme usage: {MemeId} by player: {PlayerId}", memeId, playerId);

        try
        {
            var newScore = await _memeRepository.UpdatePopularityScoreAsync(memeId, 1, cancellationToken);
            
            _logger.LogInformation("Recorded meme usage: {MemeId} by player: {PlayerId}, new popularity score: {PopularityScore}", 
                memeId, playerId, newScore);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot record usage for non-existent meme: {MemeId}", memeId);
            // Don't rethrow - this is not a critical error for game flow
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording meme usage: {MemeId} by player: {PlayerId}", memeId, playerId);
            // Don't rethrow - this is not a critical error for game flow
        }
    }

    public async Task<IReadOnlyList<string>> ValidateCategoriesAsync(
        IReadOnlyList<string> categories,
        CancellationToken cancellationToken = default)
    {
        if (categories?.Count == 0)
        {
            return [];
        }

        _logger.LogDebug("Validating categories: {Categories}", string.Join(", ", categories!));

        try
        {
            var existingCategories = await _categoryRepository.GetCategoriesByIdsAsync(categories!, cancellationToken);
            var validCategoryIds = existingCategories
                .Where(c => c.IsActive)
                .Select(c => c.Id)
                .ToList();

            if (validCategoryIds.Count != categories!.Count)
            {
                var invalidCategories = categories.Except(validCategoryIds).ToList();
                _logger.LogWarning("Invalid or inactive categories found: {InvalidCategories}", 
                    string.Join(", ", invalidCategories));
            }

            _logger.LogDebug("Validated {ValidCount} out of {TotalCount} categories", 
                validCategoryIds.Count, categories.Count);

            return validCategoryIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating categories: {Categories}", string.Join(", ", categories!));
            throw;
        }
    }

    private async Task<IReadOnlyList<string>> GetAllActiveCategoryIdsAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetActiveCategoriesAsync(cancellationToken);
        return categories.Select(c => c.Id).ToList();
    }
}
