using HexMaster.MemeIt.Memes.Abstractions;
using HexMaster.MemeIt.Memes.Models;
using HexMaster.MemeIt.Memes.Models.Entities;
using HexMaster.MemeIt.Memes.Models.Mappers;
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
        var entity = MemeTemplateMapper.ToEntity(memeTemplate);
        var response = await _container.CreateItemAsync(entity, new PartitionKey(entity.PartitionKey), cancellationToken: cancellationToken);
        var createdMemeTemplate = MemeTemplateMapper.ToDomainModel(response.Resource);
        createdMemeTemplate.SetETag(response.ETag);
        return createdMemeTemplate;
    }

    public async Task<MemeTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<MemeTemplateEntity>(id, new PartitionKey(MemesConstants.CosmosDbPartitionKey), cancellationToken: cancellationToken);
            var memeTemplate = MemeTemplateMapper.ToDomainModel(response.Resource);
            memeTemplate.SetETag(response.ETag);
            return memeTemplate;
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
        
        var iterator = _container.GetItemQueryIterator<MemeTemplateEntity>(query);
        var results = new List<MemeTemplate>();
        
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            var domainModels = response.Select(entity =>
            {
                var memeTemplate = MemeTemplateMapper.ToDomainModel(entity);
                memeTemplate.SetETag(entity.ETag);
                return memeTemplate;
            });
            results.AddRange(domainModels);
        }
        
        return results;
    }

    public async Task<MemeTemplate> UpdateAsync(MemeTemplate memeTemplate, CancellationToken cancellationToken = default)
    {
        var entity = MemeTemplateMapper.ToEntity(memeTemplate);
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        var response = await _container.ReplaceItemAsync(entity, entity.Id, new PartitionKey(entity.PartitionKey), cancellationToken: cancellationToken);
        var updatedMemeTemplate = MemeTemplateMapper.ToDomainModel(response.Resource);
        updatedMemeTemplate.SetETag(response.ETag);
        return updatedMemeTemplate;
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _container.DeleteItemAsync<MemeTemplateEntity>(id, new PartitionKey(MemesConstants.CosmosDbPartitionKey), cancellationToken: cancellationToken);
    }
}
