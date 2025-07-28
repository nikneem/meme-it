using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.Models;
using Microsoft.Azure.Cosmos;

namespace HexMaster.MemeIt.Memes.Repositories;

public class MemeTemplateRepository : IMemeTemplateRepository
{
    private readonly Container _container;

    public MemeTemplateRepository(Container container)
    {
        _container = container;
    }

    public async Task<MemeTemplate> CreateAsync(MemeTemplate memeTemplate, CancellationToken cancellationToken = default)
    {
        var response = await _container.CreateItemAsync(memeTemplate, new PartitionKey(memeTemplate.PartitionKey), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task<MemeTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<MemeTemplate>(id, new PartitionKey(MemesConstants.CosmosDbPartitionKey), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<MemeTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.partitionKey = @partitionKey")
            .WithParameter("@partitionKey", MemesConstants.CosmosDbPartitionKey);
        
        var iterator = _container.GetItemQueryIterator<MemeTemplate>(query);
        var results = new List<MemeTemplate>();
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }
        
        return results;
    }

    public async Task<MemeTemplate> UpdateAsync(MemeTemplate memeTemplate, CancellationToken cancellationToken = default)
    {
        memeTemplate.UpdatedAt = DateTime.UtcNow;
        var response = await _container.ReplaceItemAsync(memeTemplate, memeTemplate.Id, new PartitionKey(memeTemplate.PartitionKey), cancellationToken: cancellationToken);
        return response.Resource;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<MemeTemplate>(id, new PartitionKey(MemesConstants.CosmosDbPartitionKey), cancellationToken: cancellationToken);
    }
}
