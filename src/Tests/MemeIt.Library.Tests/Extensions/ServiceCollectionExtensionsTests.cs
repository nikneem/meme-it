using FluentAssertions;
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
        serviceProvider.GetService<IMemeRepository>().Should().NotBeNull();
        serviceProvider.GetService<IMemeCategoryRepository>().Should().NotBeNull();
        serviceProvider.GetService<IMemeLibraryService>().Should().NotBeNull();
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
                ["CosmosDb:DatabaseName"] = "test-db"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);
        services.AddLogging();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify that correct implementations are registered
        serviceProvider.GetService<IMemeRepository>().Should().BeOfType<CosmosMemeRepository>();
        serviceProvider.GetService<IMemeCategoryRepository>().Should().BeOfType<CosmosMemeCategoryRepository>();
        serviceProvider.GetService<IMemeLibraryService>().Should().BeOfType<MemeLibraryService>();
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
        var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

        options.Should().NotBeNull();
        options.ConnectionString.Should().Be("AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;");
        options.DatabaseName.Should().Be("test-database");
        options.MemesContainerName.Should().Be("test-memes");
        options.CategoriesContainerName.Should().Be("test-categories");
        options.MaxRetryAttempts.Should().Be(5);
        options.MaxRetryWaitTimeInSeconds.Should().Be(60);
        options.CreateIfNotExists.Should().BeTrue();
        options.MemesContainerThroughput.Should().Be(1000);
        options.CategoriesContainerThroughput.Should().Be(800);
    }

    [Fact]
    public void AddMemeLibrary_WithMinimalConfiguration_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;",
                ["CosmosDb:DatabaseName"] = "test-database"
            })
            .Build();

        // Act
        services.AddMemeLibrary(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

        options.Should().NotBeNull();
        options.MemesContainerName.Should().Be("memes");
        options.CategoriesContainerName.Should().Be("categories");
        options.MaxRetryAttempts.Should().Be(3);
        options.MaxRetryWaitTimeInSeconds.Should().Be(30);
        options.CreateIfNotExists.Should().BeTrue();
        options.MemesContainerThroughput.Should().Be(400);
        options.CategoriesContainerThroughput.Should().Be(400);
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
        var repositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMemeRepository));
        repositoryDescriptor.Should().NotBeNull();
        repositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var categoryRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMemeCategoryRepository));
        categoryRepositoryDescriptor.Should().NotBeNull();
        categoryRepositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var serviceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMemeLibraryService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMemeLibraryWithAspire_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMemeLibraryWithAspire();

        // Assert
        var repositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMemeRepository));
        repositoryDescriptor.Should().NotBeNull();
        repositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var categoryRepositoryDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMemeCategoryRepository));
        categoryRepositoryDescriptor.Should().NotBeNull();
        categoryRepositoryDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var serviceDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IMemeLibraryService));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMemeLibraryWithAspire_ConfiguresDefaultOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMemeLibraryWithAspire();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CosmosDbOptions>>().Value;

        options.Should().NotBeNull();
        options.DatabaseName.Should().Be("memeit");
        options.MemesContainerName.Should().Be("memes");
        options.CategoriesContainerName.Should().Be("categories");
        options.MaxRetryAttempts.Should().Be(3);
        options.MaxRetryWaitTimeInSeconds.Should().Be(30);
        options.CreateIfNotExists.Should().BeTrue();
        options.MemesContainerThroughput.Should().Be(400);
        options.CategoriesContainerThroughput.Should().Be(400);
    }
}
