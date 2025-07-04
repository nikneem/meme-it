using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MemeIt.Library.Abstractions;
using MemeIt.Library.Extensions;
using MemeIt.Library.Infrastructure.Configuration;
using MemeIt.Library.Infrastructure.Repositories;
using MemeIt.Library.Services;
using Moq;

namespace MemeIt.Library.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMemeLibrary_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["CosmosDb:DatabaseName"] = "test-db",
                ["CosmosDb:MemesContainerName"] = "memes",
                ["CosmosDb:CategoriesContainerName"] = "categories"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);
        services.AddLogging();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify that all required services are registered
        Assert.NotNull(serviceProvider.GetService<IMemeRepository>());
        Assert.NotNull(serviceProvider.GetService<IMemeCategoryRepository>());
        Assert.NotNull(serviceProvider.GetService<IMemeLibraryService>());
    }

    [Fact]
    public void AddMemeLibrary_RegistersCorrectImplementations()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["CosmosDb:DatabaseName"] = "test-db",
                ["CosmosDb:MemesContainerName"] = "memes",
                ["CosmosDb:CategoriesContainerName"] = "categories"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);
        services.AddLogging();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.IsType<CosmosMemeRepository>(serviceProvider.GetService<IMemeRepository>());
        Assert.IsType<CosmosMemeCategoryRepository>(serviceProvider.GetService<IMemeCategoryRepository>());
        Assert.IsType<MemeLibraryService>(serviceProvider.GetService<IMemeLibraryService>());
    }

    [Fact]
    public void AddMemeLibrary_ConfiguresCosmosDbOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["CosmosDb:DatabaseName"] = "test-database",
                ["CosmosDb:MemesContainerName"] = "test-memes",
                ["CosmosDb:CategoriesContainerName"] = "test-categories",
                ["CosmosDb:MaxRetryAttempts"] = "5",
                ["CosmosDb:MaxRetryWaitTimeInSeconds"] = "60",
                ["CosmosDb:CreateIfNotExists"] = "true",
                ["CosmosDb:MemesContainerThroughput"] = "1000",
                ["CosmosDb:CategoriesContainerThroughput"] = "800"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<CosmosDbOptions>>()?.Value;

        Assert.NotNull(options);
        Assert.Equal("AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;", options.ConnectionString);
        Assert.Equal("test-database", options.DatabaseName);
        Assert.Equal("test-memes", options.MemesContainerName);
        Assert.Equal("test-categories", options.CategoriesContainerName);
        Assert.Equal(5, options.MaxRetryAttempts);
        Assert.Equal(60, options.MaxRetryWaitTimeInSeconds);
        Assert.True(options.CreateIfNotExists);
        Assert.Equal(1000, options.MemesContainerThroughput);
        Assert.Equal(800, options.CategoriesContainerThroughput);
    }

    [Fact]
    public void AddMemeLibrary_WithDefaultConfiguration_SetsDefaultValues()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["CosmosDb:DatabaseName"] = "memeit"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<CosmosDbOptions>>()?.Value;

        Assert.NotNull(options);
        Assert.Equal("memes", options.MemesContainerName);
        Assert.Equal("categories", options.CategoriesContainerName);
        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.Equal(30, options.MaxRetryWaitTimeInSeconds);
        Assert.True(options.CreateIfNotExists);
        Assert.Equal(400, options.MemesContainerThroughput);
        Assert.Equal(400, options.CategoriesContainerThroughput);
    }

    [Fact]
    public void AddMemeLibrary_RegistersServicesWithCorrectLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["CosmosDb:DatabaseName"] = "test-db"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMemeRepository));
        Assert.NotNull(repositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor.Lifetime);

        var categoryRepositoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMemeCategoryRepository));
        Assert.NotNull(categoryRepositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, categoryRepositoryDescriptor.Lifetime);

        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMemeLibraryService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddMemeLibrary_WithConfigurationObject_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "test-connection",
                ["CosmosDb:DatabaseName"] = "test-db"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IMemeRepository));
        Assert.NotNull(repositoryDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, repositoryDescriptor.Lifetime);
    }
}
