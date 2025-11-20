using System;
using System.Threading;
using System.Threading.Tasks;
using HexMaster.MemeIt.Games.Abstractions.Domains;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Data.MongoDb.Documents;
using HexMaster.MemeIt.Games.Data.MongoDb.Mappings;
using HexMaster.MemeIt.Games.Data.MongoDb.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace HexMaster.MemeIt.Games.Data.MongoDb;

/// <summary>
/// MongoDB persistence for the games aggregate.
/// </summary>
public sealed class MongoGamesRepository : IGamesRepository
{
    private readonly IMongoCollection<GameDocument> _collection;
    private readonly SemaphoreSlim _indexSemaphore = new(1, 1);
    private bool _indexesCreated;

    public MongoGamesRepository(IMongoClient client, IOptions<GamesMongoOptions> options)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(options);

        var settings = options.Value ?? new GamesMongoOptions();
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<GameDocument>(settings.CollectionName);
    }

    public async Task CreateAsync(IGame game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        await EnsureIndexesAsync(cancellationToken).ConfigureAwait(false);

        var document = GameDocumentMapper.ToDocument(game);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<IGame?> GetByGameCodeAsync(string gameCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gameCode))
        {
            throw new ArgumentException("Game code must be provided.", nameof(gameCode));
        }

        await EnsureIndexesAsync(cancellationToken).ConfigureAwait(false);

        var filter = Builders<GameDocument>.Filter.Eq(doc => doc.GameCode, gameCode.ToUpperInvariant());
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return document is not null ? GameDocumentMapper.FromDocument(document) : null;
    }

    public async Task UpdateAsync(IGame game, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(game);
        await EnsureIndexesAsync(cancellationToken).ConfigureAwait(false);

        var filter = Builders<GameDocument>.Filter.Eq(doc => doc.GameCode, game.GameCode);

        // Retrieve existing document to preserve the Id
        var existingDocument = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (existingDocument is null)
        {
            throw new InvalidOperationException($"Game with code '{game.GameCode}' not found for update.");
        }

        var document = GameDocumentMapper.ToDocument(game);
        document.Id = existingDocument.Id; // Preserve the MongoDB ObjectId

        var result = await _collection.ReplaceOneAsync(filter, document, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (result.MatchedCount == 0)
        {
            throw new InvalidOperationException($"Game with code '{game.GameCode}' not found for update.");
        }
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesCreated)
        {
            return;
        }

        await _indexSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_indexesCreated)
            {
                return;
            }

            var keys = Builders<GameDocument>.IndexKeys.Ascending(doc => doc.GameCode);
            var indexModel = new CreateIndexModel<GameDocument>(keys, new CreateIndexOptions
            {
                Unique = true,
                Name = "ux_game_code"
            });

            await _collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken).ConfigureAwait(false);
            _indexesCreated = true;
        }
        finally
        {
            _indexSemaphore.Release();
        }
    }
}
