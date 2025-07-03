using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;
using MemeIt.Library.Infrastructure.Configuration;
using MemeIt.Library.Infrastructure.Models;
using System.Net;

namespace MemeIt.Library.Infrastructure.Repositories;

/// <summary>
/// CosmosDB implementation of the meme category repository
/// </summary>
public class CosmosMemeCategoryRepository : IMemeCategoryRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosMemeCategoryRepository> _logger;

    public CosmosMemeCategoryRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosMemeCategoryRepository> logger)
    {
        var cosmosOptions = options.Value;
        _container = cosmosClient.GetContainer(cosmosOptions.DatabaseName, cosmosOptions.CategoriesContainerName);
        _logger = logger;
    }

    public async Task<IReadOnlyList<MemeCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all active categories");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.type = '{nameof(MemeCategoryDocument)}' AND c.isActive = true ORDER BY c.displayOrder, c.name");
            var iterator = _container.GetItemQueryIterator<MemeCategoryDocument>(query);

            var categories = new List<MemeCategoryDocument>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                categories.AddRange(response);
            }

            _logger.LogDebug("Found {CategoryCount} active categories", categories.Count);
            return categories.Select(c => c.ToDomain()).ToList();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            throw;
        }
    }

    public async Task<MemeCategory?> GetCategoryByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting category by ID: {CategoryId}", id);

            var response = await _container.ReadItemAsync<MemeCategoryDocument>(id, new PartitionKey("category"), cancellationToken: cancellationToken);
            return response.Resource.ToDomain();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Category not found: {CategoryId}", id);
            return null;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<MemeCategory>> GetCategoriesByIdsAsync(
        IReadOnlyList<string> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        try
        {
            _logger.LogDebug("Getting categories by IDs: {CategoryIds}", string.Join(", ", ids));

            var idConditions = ids.Select((_, index) => $"@id{index}").ToList();
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.type = '{nameof(MemeCategoryDocument)}' AND c.id IN ({string.Join(", ", idConditions)})");
            
            for (int i = 0; i < ids.Count; i++)
            {
                query = query.WithParameter($"@id{i}", ids[i]);
            }

            var iterator = _container.GetItemQueryIterator<MemeCategoryDocument>(query);
            var categories = new List<MemeCategoryDocument>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                categories.AddRange(response);
            }

            _logger.LogDebug("Found {CategoryCount} categories out of {RequestedCount} requested", 
                categories.Count, ids.Count);

            return categories.Select(c => c.ToDomain()).ToList();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting categories by IDs: {CategoryIds}", string.Join(", ", ids));
            throw;
        }
    }

    public async Task<MemeCategory> CreateCategoryAsync(MemeCategory category, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating category: {CategoryId}", category.Id);

            var document = MemeCategoryDocument.FromDomain(category);
            var response = await _container.CreateItemAsync(document, new PartitionKey("category"), cancellationToken: cancellationToken);

            _logger.LogInformation("Created category: {CategoryId}", category.Id);
            return response.Resource.ToDomain();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating category: {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task<MemeCategory> UpdateCategoryAsync(MemeCategory category, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating category: {CategoryId}", category.Id);

            var document = MemeCategoryDocument.FromDomain(category);
            var response = await _container.UpsertItemAsync(document, new PartitionKey("category"), cancellationToken: cancellationToken);

            _logger.LogInformation("Updated category: {CategoryId}", category.Id);
            return response.Resource.ToDomain();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error updating category: {CategoryId}", category.Id);
            throw;
        }
    }

    public async Task<bool> DeleteCategoryAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting category: {CategoryId}", id);

            await _container.DeleteItemAsync<MemeCategoryDocument>(id, new PartitionKey("category"), cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted category: {CategoryId}", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Category not found for deletion: {CategoryId}", id);
            return false;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
            throw;
        }
    }
}
