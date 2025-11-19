using System;
using HexMaster.MemeIt.Games.Abstractions.Repositories;
using HexMaster.MemeIt.Games.Data.MongoDb.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HexMaster.MemeIt.Games.Data.MongoDb;

/// <summary>
/// Dependency injection helpers for Mongo-based persistence.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGamesMongoData(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<GamesMongoOptions>()
            .Bind(configuration.GetSection(GamesMongoOptions.SectionName))
            .PostConfigure(options =>
            {
                options.DatabaseName = string.IsNullOrWhiteSpace(options.DatabaseName)
                    ? GamesMongoOptions.DefaultDatabaseName
                    : options.DatabaseName;

                options.CollectionName = string.IsNullOrWhiteSpace(options.CollectionName)
                    ? GamesMongoOptions.DefaultCollectionName
                    : options.CollectionName;
            });

        services.AddSingleton<IGamesRepository, MongoGamesRepository>();
        return services;
    }
}
