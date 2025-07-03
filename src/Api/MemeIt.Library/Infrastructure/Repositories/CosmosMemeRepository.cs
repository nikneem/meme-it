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

            var queryDefinition = BuildRandomMemeQuery(categories, excludedMemeIds ?? []);
            var iterator = _container.GetItemQueryIterator<MemeDocument>(queryDefinition);

            var memes = new List<MemeDocument>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
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

            var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id AND c.type = 'meme'")
                .WithParameter("@id", id);

            var iterator = _container.GetItemQueryIterator<MemeDocument>(query);
            var response = await iterator.ReadNextAsync(cancellationToken);

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

            var queryDefinition = BuildCategoryMemeQuery(categories, isActive);
            var iterator = _container.GetItemQueryIterator<MemeDocument>(queryDefinition);

            var memes = new List<MemeDocument>();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
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

            var partitionKey = meme.Categories.Count > 0 ? meme.Categories[0] : "default";
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

    private static QueryDefinition BuildRandomMemeQuery(IReadOnlyList<string> categories, IReadOnlyList<string> excludedMemeIds)
    {
        var query = "SELECT * FROM c WHERE c.type = 'meme' AND c.isActive = true";
        var parameters = new List<(string name, object value)>();

        if (categories.Count > 0)
        {
            var categoryConditions = categories.Select((_, index) => $"ARRAY_CONTAINS(c.categories, @category{index})").ToList();
            query += $" AND ({string.Join(" OR ", categoryConditions)})";
            
            for (int i = 0; i < categories.Count; i++)
            {
                parameters.Add(($"@category{i}", categories[i]));
            }
        }

        if (excludedMemeIds.Count > 0)
        {
            var excludeConditions = excludedMemeIds.Select((_, index) => $"@exclude{index}").ToList();
            query += $" AND c.id NOT IN ({string.Join(", ", excludeConditions)})";
            
            for (int i = 0; i < excludedMemeIds.Count; i++)
            {
                parameters.Add(($"@exclude{i}", excludedMemeIds[i]));
            }
        }

        var queryDefinition = new QueryDefinition(query);
        foreach (var (name, value) in parameters)
        {
            queryDefinition = queryDefinition.WithParameter(name, value);
        }

        return queryDefinition;
    }

    private static QueryDefinition BuildCategoryMemeQuery(IReadOnlyList<string> categories, bool? isActive)
    {
        var query = "SELECT * FROM c WHERE c.type = 'meme'";
        var parameters = new List<(string name, object value)>();

        if (isActive.HasValue)
        {
            query += " AND c.isActive = @isActive";
            parameters.Add(("@isActive", isActive.Value));
        }

        if (categories.Count > 0)
        {
            var categoryConditions = categories.Select((_, index) => $"ARRAY_CONTAINS(c.categories, @category{index})").ToList();
            query += $" AND ({string.Join(" OR ", categoryConditions)})";
            
            for (int i = 0; i < categories.Count; i++)
            {
                parameters.Add(($"@category{i}", categories[i]));
            }
        }

        var queryDefinition = new QueryDefinition(query);
        foreach (var (name, value) in parameters)
        {
            queryDefinition = queryDefinition.WithParameter(name, value);
        }

        return queryDefinition;
    }
}
