using Microsoft.AspNetCore.Mvc;
using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;

namespace MemeIt.Api.Controllers;

/// <summary>
/// Controller for meme category operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IMemeLibraryService _memeLibraryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IMemeLibraryService memeLibraryService, ILogger<CategoriesController> logger)
    {
        _memeLibraryService = memeLibraryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all available categories
    /// </summary>
    /// <returns>List of available categories</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MemeCategory>>> GetAvailableCategories()
    {
        try
        {
            var categories = await _memeLibraryService.GetAvailableCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Gets a category by ID (Note: This requires direct repository access as the service doesn't expose this method)
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <returns>The category</returns>
    [HttpGet("{id}")]
    public ActionResult<MemeCategory> GetCategory(string id)
    {
        try
        {
            // Note: The service interface doesn't have GetCategoryByIdAsync
            // This would require either:
            // 1. Adding the method to IMemeLibraryService
            // 2. Injecting IMemeCategoryRepository directly
            // 3. Removing this endpoint
            return NotFound("This endpoint is not implemented. Use /api/categories to get all categories.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    /// <summary>
    /// Validates that the specified category IDs exist and are active
    /// </summary>
    /// <param name="categoryIds">Comma-separated list of category IDs to validate</param>
    /// <returns>Validation result with list of valid categories</returns>
    [HttpPost("validate")]
    public async Task<ActionResult<IReadOnlyList<string>>> ValidateCategories([FromQuery] string categoryIds)
    {
        try
        {
            if (string.IsNullOrEmpty(categoryIds))
            {
                return BadRequest("Category IDs are required");
            }

            var categoryList = categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var validCategories = await _memeLibraryService.ValidateCategoriesAsync(categoryList);
            
            return Ok(validCategories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating categories {CategoryIds}", categoryIds);
            return StatusCode(500, "An error occurred while validating categories");
        }
    }
}
