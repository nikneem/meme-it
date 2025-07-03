using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MemeIt.Core.Models;
using MemeIt.Library.Infrastructure.Configuration;
using MemeIt.Library.Infrastructure.Models;
using MemeIt.Library.Infrastructure.Repositories;
using MemeIt.Library.Tests.TestData;
using System.Net;

namespace MemeIt.Library.Tests.Infrastructure;

public class CosmosMemeCategoryRepositoryTests
{
    private readonly Mock<CosmosClient> _cosmosClientMock;
    private readonly Mock<Container> _containerMock;
    private readonly Mock<IOptions<CosmosDbOptions>> _optionsMock;
    private readonly Mock<ILogger<CosmosMemeCategoryRepository>> _loggerMock;
    private readonly CosmosMemeCategoryRepository _sut;
    private readonly CosmosDbOptions _cosmosOptions;

    public CosmosMemeCategoryRepositoryTests()
    {
        _cosmosClientMock = new Mock<CosmosClient>();
        _containerMock = new Mock<Container>();
        _optionsMock = new Mock<IOptions<CosmosDbOptions>>();
        _loggerMock = new Mock<ILogger<CosmosMemeCategoryRepository>>();

        _cosmosOptions = new CosmosDbOptions
        {
            ConnectionString = "test-connection",
            DatabaseName = "test-db",
            CategoriesContainerName = "categories"
        };

        _optionsMock.Setup(x => x.Value).Returns(_cosmosOptions);
        _cosmosClientMock.Setup(x => x.GetContainer(_cosmosOptions.DatabaseName, _cosmosOptions.CategoriesContainerName))
            .Returns(_containerMock.Object);

        _sut = new CosmosMemeCategoryRepository(_cosmosClientMock.Object, _optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetActiveCategoriesAsync_ReturnsActiveCategories()
    {
        // Arrange
        var activeCategories = MemeTestDataFactory.CreateMultipleCategories()
            .Where(c => c.IsActive)
            .ToList();
        var categoryDocuments = activeCategories.Select(MemeCategoryDocument.FromDomain).ToList();

        var mockIterator = new Mock<FeedIterator<MemeCategoryDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeCategoryDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(categoryDocuments.GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeCategoryDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.GetActiveCategoriesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(activeCategories.Count);
        result.All(c => c.IsActive).Should().BeTrue();
        result.Should().BeEquivalentTo(activeCategories);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithValidId_ReturnsCategory()
    {
        // Arrange
        var categoryId = "humor";
        var expectedCategory = MemeTestDataFactory.CreateSampleCategory(categoryId);
        var categoryDocument = MemeCategoryDocument.FromDomain(expectedCategory);

        var mockResponse = new Mock<ItemResponse<MemeCategoryDocument>>();
        mockResponse.Setup(x => x.Resource).Returns(categoryDocument);

        _containerMock.Setup(x => x.ReadItemAsync<MemeCategoryDocument>(
                categoryId,
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _sut.GetCategoryByIdAsync(categoryId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(categoryId);
        result.Should().BeEquivalentTo(expectedCategory);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var categoryId = "non-existent";

        _containerMock.Setup(x => x.ReadItemAsync<MemeCategoryDocument>(
                categoryId,
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _sut.GetCategoryByIdAsync(categoryId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoriesByIdsAsync_WithValidIds_ReturnsMatchingCategories()
    {
        // Arrange
        var categoryIds = new List<string> { "humor", "classic", "animals" };
        var allCategories = MemeTestDataFactory.CreateMultipleCategories();
        var matchingCategories = allCategories.Where(c => categoryIds.Contains(c.Id)).ToList();
        var categoryDocuments = matchingCategories.Select(MemeCategoryDocument.FromDomain).ToList();

        var mockIterator = new Mock<FeedIterator<MemeCategoryDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeCategoryDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(categoryDocuments.GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeCategoryDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.GetCategoriesByIdsAsync(categoryIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(matchingCategories.Count);
        result.Select(c => c.Id).Should().BeSubsetOf(categoryIds);
        result.Should().BeEquivalentTo(matchingCategories);
    }

    [Fact]
    public async Task GetCategoriesByIdsAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var emptyIds = new List<string>();

        // Act
        var result = await _sut.GetCategoriesByIdsAsync(emptyIds);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
        _containerMock.Verify(x => x.GetItemQueryIterator<MemeCategoryDocument>(
            It.IsAny<QueryDefinition>(), null, null), Times.Never);
    }

    [Fact]
    public async Task CreateCategoryAsync_WithValidCategory_ReturnsCreatedCategory()
    {
        // Arrange
        var category = MemeTestDataFactory.CreateSampleCategory();
        var categoryDocument = MemeCategoryDocument.FromDomain(category);

        var mockResponse = new Mock<ItemResponse<MemeCategoryDocument>>();
        mockResponse.Setup(x => x.Resource).Returns(categoryDocument);

        _containerMock.Setup(x => x.CreateItemAsync(
                It.IsAny<MemeCategoryDocument>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _sut.CreateCategoryAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(category);
        _containerMock.Verify(x => x.CreateItemAsync(
            It.IsAny<MemeCategoryDocument>(),
            new PartitionKey("category"),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WithValidCategory_ReturnsUpdatedCategory()
    {
        // Arrange
        var category = MemeTestDataFactory.CreateSampleCategory();
        var categoryDocument = MemeCategoryDocument.FromDomain(category);

        var mockResponse = new Mock<ItemResponse<MemeCategoryDocument>>();
        mockResponse.Setup(x => x.Resource).Returns(categoryDocument);

        _containerMock.Setup(x => x.UpsertItemAsync(
                It.IsAny<MemeCategoryDocument>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _sut.UpdateCategoryAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(category);
        _containerMock.Verify(x => x.UpsertItemAsync(
            It.IsAny<MemeCategoryDocument>(),
            new PartitionKey("category"),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithExistingCategory_ReturnsTrue()
    {
        // Arrange
        var categoryId = "humor";

        var mockResponse = new Mock<ItemResponse<MemeCategoryDocument>>();
        _containerMock.Setup(x => x.DeleteItemAsync<MemeCategoryDocument>(
                categoryId,
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _sut.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeTrue();
        _containerMock.Verify(x => x.DeleteItemAsync<MemeCategoryDocument>(
            categoryId,
            new PartitionKey("category"),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithNonExistentCategory_ReturnsFalse()
    {
        // Arrange
        var categoryId = "non-existent";

        _containerMock.Setup(x => x.DeleteItemAsync<MemeCategoryDocument>(
                categoryId,
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not found", HttpStatusCode.NotFound, 0, "", 0));

        // Act
        var result = await _sut.DeleteCategoryAsync(categoryId);

        // Assert
        result.Should().BeFalse();
    }
}
