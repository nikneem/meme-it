using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MemeIt.Library.Abstractions;
using MemeIt.Library.Infrastructure.Configuration;
using MemeIt.Library.Infrastructure.Repositories;
using MemeIt.Library.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MemeIt.Library.Extensions;

/// <summary>
/// Extension methods for dependency injection configuration
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the MemeIt Library services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMemeLibrary(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure CosmosDB options
        services.Configure<CosmosDbOptions>(configuration.GetSection(CosmosDbOptions.SectionName));

        // Register CosmosClient as singleton only if not already registered (for Aspire compatibility)
        if (services.All(x => x.ServiceType != typeof(CosmosClient)))
        {
            services.TryAddSingleton<CosmosClient>(serviceProvider =>
            {
                var cosmosOptions = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;
                
                // Validate connection string
                if (string.IsNullOrEmpty(cosmosOptions.ConnectionString))
                {
                    throw new InvalidOperationException("CosmosDB connection string is required but not configured.");
                }

                var cosmosClientOptions = new CosmosClientOptions
                {
                    MaxRetryAttemptsOnRateLimitedRequests = cosmosOptions.MaxRetryAttempts,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(cosmosOptions.MaxRetryWaitTimeInSeconds),
                    ConnectionMode = ConnectionMode.Direct,
                    ConsistencyLevel = ConsistencyLevel.Session
                };

                return new CosmosClient(cosmosOptions.ConnectionString, cosmosClientOptions);
            });
        }

        // Register repositories
        services.AddScoped<IMemeRepository, CosmosMemeRepository>();
        services.AddScoped<IMemeCategoryRepository, CosmosMemeCategoryRepository>();

        // Register services
        services.AddScoped<IMemeLibraryService, MemeLibraryService>();

        return services;
    }

    /// <summary>
    /// Adds the MemeIt Library services to the dependency injection container with Aspire integration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionName">The name of the connection for Aspire</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMemeLibraryWithAspire(this IServiceCollection services, string connectionName = "cosmos")
    {
        // Register repositories
        services.AddScoped<IMemeRepository, CosmosMemeRepository>();
        services.AddScoped<IMemeCategoryRepository, CosmosMemeCategoryRepository>();

        // Register services
        services.AddScoped<IMemeLibraryService, MemeLibraryService>();

        // Configure CosmosDB options for Aspire
        services.Configure<CosmosDbOptions>(options =>
        {
            options.DatabaseName = "memeit";
            options.MemesContainerName = "memes";
            options.CategoriesContainerName = "categories";
            options.MaxRetryAttempts = 3;
            options.MaxRetryWaitTimeInSeconds = 30;
            options.CreateIfNotExists = true;
            options.MemesContainerThroughput = 400;
            options.CategoriesContainerThroughput = 400;
        });

        return services;
    }

    /// <summary>
    /// Initializes the CosmosDB database and containers if they don't exist
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <returns>Task representing the initialization operation</returns>
    public static async Task InitializeMemeLibraryDatabaseAsync(this IServiceProvider serviceProvider)
    {
        var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
        var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

        if (!options.CreateIfNotExists)
        {
            return;
        }

        try
        {
            // Create database if it doesn't exist
            var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
                options.DatabaseName,
                ThroughputProperties.CreateAutoscaleThroughput(1000));

            var database = databaseResponse.Database;

            // Create memes container if it doesn't exist
            var memesContainerProperties = new ContainerProperties(options.MemesContainerName, "/partitionKey")
            {
                IndexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true
                }
            };

            // Add specific indexes for efficient querying
            memesContainerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/categories/*" });
            memesContainerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/isActive/*" });
            memesContainerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/type/*" });

            await database.CreateContainerIfNotExistsAsync(
                memesContainerProperties,
                ThroughputProperties.CreateManualThroughput(options.MemesContainerThroughput));

            // Create categories container if it doesn't exist
            var categoriesContainerProperties = new ContainerProperties(options.CategoriesContainerName, "/partitionKey")
            {
                IndexingPolicy = new IndexingPolicy
                {
                    IndexingMode = IndexingMode.Consistent,
                    Automatic = true
                }
            };

            // Add specific indexes for efficient querying
            categoriesContainerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/isActive/*" });
            categoriesContainerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/type/*" });
            categoriesContainerProperties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/displayOrder/*" });

            await database.CreateContainerIfNotExistsAsync(
                categoriesContainerProperties,
                ThroughputProperties.CreateManualThroughput(options.CategoriesContainerThroughput));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize MemeLibrary database", ex);
        }
    }
}
