using MemeIt.Core.Models;

namespace MemeIt.Library.Abstractions;

/// <summary>
/// Defines the contract for meme category repository operations
/// </summary>
public interface IMemeCategoryRepository
{
    /// <summary>
    /// Gets all active meme categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active categories</returns>
    Task<IReadOnlyList<MemeCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by its identifier
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category or null if not found</returns>
    Task<MemeCategory?> GetCategoryByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets multiple categories by their identifiers
    /// </summary>
    /// <param name="ids">The category identifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of found categories</returns>
    Task<IReadOnlyList<MemeCategory>> GetCategoriesByIdsAsync(
        IReadOnlyList<string> ids, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="category">The category to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created category</returns>
    Task<MemeCategory> CreateCategoryAsync(MemeCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="category">The category to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated category</returns>
    Task<MemeCategory> UpdateCategoryAsync(MemeCategory category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category by its identifier
    /// </summary>
    /// <param name="id">The category identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteCategoryAsync(string id, CancellationToken cancellationToken = default);
}
