using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;
using MemeIt.Library.Infrastructure.Configuration;
using MemeIt.Library.Infrastructure.Constants;
using MemeIt.Library.Infrastructure.Models;
using System.Net;

namespace MemeIt.Library.Infrastructure.Repositories;

/// <summary>
/// CosmosDB implementation of the meme repository
/// </summary>
public class CosmosMemeRepository : IMemeRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosMemeRepository> _logger;
    private readonly Random _random = new();

    public CosmosMemeRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> options,
        ILogger<CosmosMemeRepository> logger)
    {
        var cosmosOptions = options.Value;
        _container = cosmosClient.GetContainer(cosmosOptions.DatabaseName, cosmosOptions.MemesContainerName);
        _logger = logger;
    }

    public async Task<Meme?> GetRandomMemeAsync(
        IReadOnlyList<string> categories,
        IReadOnlyList<string> excludedMemeIds = null!,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting random meme for categories: {Categories}", string.Join(", ", categories));

            var categoryFilter = categories.Count == 0 ? 
                string.Empty : 
                $" AND ARRAY_CONTAINS(@{CosmosDbConstants.Parameters.Categories}, c.{CosmosDbConstants.FieldNames.Category})";

            var excludeFilter = excludedMemeIds?.Count > 0 ? 
                $" AND NOT ARRAY_CONTAINS(@{CosmosDbConstants.Parameters.ExcludedIds}, c.id)" : 
                string.Empty;

            var sql = $@"
                SELECT * FROM c 
                WHERE c.{CosmosDbConstants.FieldNames.Type} = @{CosmosDbConstants.Parameters.Type}
                AND c.{CosmosDbConstants.FieldNames.IsActive} = true
                {categoryFilter}
                {excludeFilter}";

            var queryDefinition = new QueryDefinition(sql)
                .WithParameter($"@{CosmosDbConstants.Parameters.Type}", CosmosDbConstants.DocumentTypes.Meme);

            if (categories.Count > 0)
            {
                queryDefinition = queryDefinition.WithParameter($"@{CosmosDbConstants.Parameters.Categories}", categories);
            }

            if (excludedMemeIds?.Count > 0)
            {
                queryDefinition = queryDefinition.WithParameter($"@{CosmosDbConstants.Parameters.ExcludedIds}", excludedMemeIds);
            }

            using var feedIterator = _container.GetItemQueryIterator<MemeDocument>(queryDefinition);

            var memes = new List<MemeDocument>();
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                memes.AddRange(response);
            }

            if (memes.Count == 0)
            {
                _logger.LogWarning("No memes found for categories: {Categories}", string.Join(", ", categories));
                return null;
            }

            var randomIndex = _random.Next(memes.Count);
            var selectedMeme = memes[randomIndex];

            _logger.LogDebug("Selected random meme: {MemeId} from {TotalMemes} available memes", 
                selectedMeme.Id, memes.Count);

            return selectedMeme.ToDomain();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting random meme for categories: {Categories}", string.Join(", ", categories));
            throw;
        }
    }

    public async Task<Meme?> GetMemeByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting meme by ID: {MemeId}", id);

            var sql = $@"
                SELECT * FROM c 
                WHERE c.id = @{CosmosDbConstants.Parameters.Id} 
                AND c.{CosmosDbConstants.FieldNames.Type} = @{CosmosDbConstants.Parameters.Type}";

            var queryDefinition = new QueryDefinition(sql)
                .WithParameter($"@{CosmosDbConstants.Parameters.Id}", id)
                .WithParameter($"@{CosmosDbConstants.Parameters.Type}", CosmosDbConstants.DocumentTypes.Meme);

            using var feedIterator = _container.GetItemQueryIterator<MemeDocument>(queryDefinition);
            var response = await feedIterator.ReadNextAsync(cancellationToken);

            var meme = response.FirstOrDefault();
            if (meme == null)
            {
                _logger.LogWarning("Meme not found: {MemeId}", id);
                return null;
            }

            return meme.ToDomain();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting meme by ID: {MemeId}", id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Meme>> GetMemesByCategoriesAsync(
        IReadOnlyList<string> categories,
        bool? isActive = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting memes by categories: {Categories}", string.Join(", ", categories));

            var categoryFilter = categories.Count == 0 ? 
                string.Empty : 
                $" AND ARRAY_CONTAINS(@{CosmosDbConstants.Parameters.Categories}, c.{CosmosDbConstants.FieldNames.Category})";

            var activeFilter = isActive.HasValue ? 
                $" AND c.{CosmosDbConstants.FieldNames.IsActive} = @{CosmosDbConstants.Parameters.IsActive}" : 
                string.Empty;

            var sql = $@"
                SELECT * FROM c 
                WHERE c.{CosmosDbConstants.FieldNames.Type} = @{CosmosDbConstants.Parameters.Type}
                {categoryFilter}
                {activeFilter}";

            var queryDefinition = new QueryDefinition(sql)
                .WithParameter($"@{CosmosDbConstants.Parameters.Type}", CosmosDbConstants.DocumentTypes.Meme);

            if (categories.Count > 0)
            {
                queryDefinition = queryDefinition.WithParameter($"@{CosmosDbConstants.Parameters.Categories}", categories);
            }

            if (isActive.HasValue)
            {
                queryDefinition = queryDefinition.WithParameter($"@{CosmosDbConstants.Parameters.IsActive}", isActive.Value);
            }

            using var feedIterator = _container.GetItemQueryIterator<MemeDocument>(queryDefinition);
            var memes = new List<MemeDocument>();
            
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync(cancellationToken);
                memes.AddRange(response);
            }

            _logger.LogDebug("Found {MemeCount} memes for categories: {Categories}", 
                memes.Count, string.Join(", ", categories));

            return memes.Select(m => m.ToDomain()).ToList();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error getting memes by categories: {Categories}", string.Join(", ", categories));
            throw;
        }
    }

    public async Task<Meme> CreateMemeAsync(Meme meme, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating meme: {MemeId}", meme.Id);

            var document = MemeDocument.FromDomain(meme);
            var response = await _container.CreateItemAsync(document, new PartitionKey(document.PartitionKey), cancellationToken: cancellationToken);

            _logger.LogInformation("Created meme: {MemeId}", meme.Id);
            return response.Resource.ToDomain();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error creating meme: {MemeId}", meme.Id);
            throw;
        }
    }

    public async Task<Meme> UpdateMemeAsync(Meme meme, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating meme: {MemeId}", meme.Id);

            var document = MemeDocument.FromDomain(meme with { ModifiedAt = DateTimeOffset.UtcNow });
            var response = await _container.UpsertItemAsync(document, new PartitionKey(document.PartitionKey), cancellationToken: cancellationToken);

            _logger.LogInformation("Updated meme: {MemeId}", meme.Id);
            return response.Resource.ToDomain();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error updating meme: {MemeId}", meme.Id);
            throw;
        }
    }

    public async Task<bool> DeleteMemeAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting meme: {MemeId}", id);

            // First get the meme to determine the partition key
            var meme = await GetMemeByIdAsync(id, cancellationToken);
            if (meme == null)
            {
                _logger.LogWarning("Meme not found for deletion: {MemeId}", id);
                return false;
            }

            var partitionKey = meme.Categories.Count > 0 ? meme.Categories[0] : CosmosDbConstants.PartitionKeys.Default;
            await _container.DeleteItemAsync<MemeDocument>(id, new PartitionKey(partitionKey), cancellationToken: cancellationToken);

            _logger.LogInformation("Deleted meme: {MemeId}", id);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Meme not found for deletion: {MemeId}", id);
            return false;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error deleting meme: {MemeId}", id);
            throw;
        }
    }

    public async Task<int> UpdatePopularityScoreAsync(string memeId, int increment = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating popularity score for meme: {MemeId} by {Increment}", memeId, increment);

            var meme = await GetMemeByIdAsync(memeId, cancellationToken);
            if (meme == null)
            {
                _logger.LogWarning("Meme not found for popularity update: {MemeId}", memeId);
                throw new InvalidOperationException($"Meme with ID {memeId} not found");
            }

            var updatedMeme = meme with { PopularityScore = meme.PopularityScore + increment };
            await UpdateMemeAsync(updatedMeme, cancellationToken);

            _logger.LogDebug("Updated popularity score for meme: {MemeId} to {NewScore}", memeId, updatedMeme.PopularityScore);
            return updatedMeme.PopularityScore;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error updating popularity score for meme: {MemeId}", memeId);
            throw;
        }
    }
}
