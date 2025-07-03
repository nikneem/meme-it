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

public class CosmosMemeRepositoryTests
{
    private readonly Mock<CosmosClient> _cosmosClientMock;
    private readonly Mock<Container> _containerMock;
    private readonly Mock<IOptions<CosmosDbOptions>> _optionsMock;
    private readonly Mock<ILogger<CosmosMemeRepository>> _loggerMock;
    private readonly CosmosMemeRepository _sut;
    private readonly CosmosDbOptions _cosmosOptions;

    public CosmosMemeRepositoryTests()
    {
        _cosmosClientMock = new Mock<CosmosClient>();
        _containerMock = new Mock<Container>();
        _optionsMock = new Mock<IOptions<CosmosDbOptions>>();
        _loggerMock = new Mock<ILogger<CosmosMemeRepository>>();

        _cosmosOptions = new CosmosDbOptions
        {
            ConnectionString = "test-connection",
            DatabaseName = "test-db",
            MemesContainerName = "memes"
        };

        _optionsMock.Setup(x => x.Value).Returns(_cosmosOptions);
        _cosmosClientMock.Setup(x => x.GetContainer(_cosmosOptions.DatabaseName, _cosmosOptions.MemesContainerName))
            .Returns(_containerMock.Object);

        _sut = new CosmosMemeRepository(_cosmosClientMock.Object, _optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetMemeByIdAsync_WithValidId_ReturnsMeme()
    {
        // Arrange
        var memeId = "test-meme-1";
        var expectedMeme = MemeTestDataFactory.CreateSampleMeme(memeId);
        var memeDocument = MemeDocument.FromDomain(expectedMeme);

        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument> { memeDocument }.GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.GetMemeByIdAsync(memeId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(memeId);
        result.Should().BeEquivalentTo(expectedMeme);
    }

    [Fact]
    public async Task GetMemeByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var memeId = "non-existent-meme";

        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument>().GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.GetMemeByIdAsync(memeId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateMemeAsync_WithValidMeme_ReturnsCreatedMeme()
    {
        // Arrange
        var meme = MemeTestDataFactory.CreateSampleMeme();
        var memeDocument = MemeDocument.FromDomain(meme);

        var mockResponse = new Mock<ItemResponse<MemeDocument>>();
        mockResponse.Setup(x => x.Resource).Returns(memeDocument);

        _containerMock.Setup(x => x.CreateItemAsync(
                It.IsAny<MemeDocument>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _sut.CreateMemeAsync(meme);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(meme);
        _containerMock.Verify(x => x.CreateItemAsync(
            It.IsAny<MemeDocument>(),
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMemeAsync_WithValidMeme_ReturnsUpdatedMeme()
    {
        // Arrange
        var meme = MemeTestDataFactory.CreateSampleMeme();
        var memeDocument = MemeDocument.FromDomain(meme);

        var mockResponse = new Mock<ItemResponse<MemeDocument>>();
        mockResponse.Setup(x => x.Resource).Returns(memeDocument);

        _containerMock.Setup(x => x.UpsertItemAsync(
                It.IsAny<MemeDocument>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _sut.UpdateMemeAsync(meme);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(meme);
        _containerMock.Verify(x => x.UpsertItemAsync(
            It.IsAny<MemeDocument>(),
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMemeAsync_WithExistingMeme_ReturnsTrue()
    {
        // Arrange
        var memeId = "test-meme-1";
        var existingMeme = MemeTestDataFactory.CreateSampleMeme(memeId);

        // Setup GetMemeByIdAsync to return existing meme
        var memeDocument = MemeDocument.FromDomain(existingMeme);
        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument> { memeDocument }.GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Setup DeleteItemAsync
        var mockDeleteResponse = new Mock<ItemResponse<MemeDocument>>();
        _containerMock.Setup(x => x.DeleteItemAsync<MemeDocument>(
                memeId,
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockDeleteResponse.Object);

        // Act
        var result = await _sut.DeleteMemeAsync(memeId);

        // Assert
        result.Should().BeTrue();
        _containerMock.Verify(x => x.DeleteItemAsync<MemeDocument>(
            memeId,
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMemeAsync_WithNonExistentMeme_ReturnsFalse()
    {
        // Arrange
        var memeId = "non-existent-meme";

        // Setup GetMemeByIdAsync to return null
        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument>().GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.DeleteMemeAsync(memeId);

        // Assert
        result.Should().BeFalse();
        _containerMock.Verify(x => x.DeleteItemAsync<MemeDocument>(
            It.IsAny<string>(),
            It.IsAny<PartitionKey>(),
            null,
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetRandomMemeAsync_WithCategories_ReturnsMeme()
    {
        // Arrange
        var categories = new List<string> { "humor", "classic" };
        var excludedIds = new List<string> { "excluded-1" };
        var availableMemes = MemeTestDataFactory.CreateMultipleMemes(3);

        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        var memeDocuments = availableMemes.Select(MemeDocument.FromDomain).ToList();
        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(memeDocuments.GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.GetRandomMemeAsync(categories, excludedIds);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().BeOneOf(availableMemes.Select(m => m.Id));
    }

    [Fact]
    public async Task GetRandomMemeAsync_WithNoAvailableMemes_ReturnsNull()
    {
        // Arrange
        var categories = new List<string> { "nonexistent" };

        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument>().GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act
        var result = await _sut.GetRandomMemeAsync(categories);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePopularityScoreAsync_WithValidMeme_ReturnsUpdatedScore()
    {
        // Arrange
        var memeId = "test-meme-1";
        var existingMeme = MemeTestDataFactory.CreateSampleMeme(memeId, popularityScore: 10);
        var expectedNewScore = 11;

        // Setup GetMemeByIdAsync
        var memeDocument = MemeDocument.FromDomain(existingMeme);
        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument> { memeDocument }.GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Setup UpdateMemeAsync
        var updatedMemeDocument = MemeDocument.FromDomain(existingMeme with { PopularityScore = expectedNewScore });
        var mockUpdateResponse = new Mock<ItemResponse<MemeDocument>>();
        mockUpdateResponse.Setup(x => x.Resource).Returns(updatedMemeDocument);

        _containerMock.Setup(x => x.UpsertItemAsync(
                It.IsAny<MemeDocument>(),
                It.IsAny<PartitionKey>(),
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockUpdateResponse.Object);

        // Act
        var result = await _sut.UpdatePopularityScoreAsync(memeId, 1);

        // Assert
        result.Should().Be(expectedNewScore);
    }

    [Fact]
    public async Task UpdatePopularityScoreAsync_WithNonExistentMeme_ThrowsInvalidOperationException()
    {
        // Arrange
        var memeId = "non-existent-meme";

        // Setup GetMemeByIdAsync to return null
        var mockIterator = new Mock<FeedIterator<MemeDocument>>();
        var mockResponse = new Mock<FeedResponse<MemeDocument>>();

        mockResponse.Setup(x => x.GetEnumerator())
            .Returns(new List<MemeDocument>().GetEnumerator());
        mockIterator.SetupSequence(x => x.HasMoreResults)
            .Returns(true)
            .Returns(false);
        mockIterator.Setup(x => x.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _containerMock.Setup(x => x.GetItemQueryIterator<MemeDocument>(It.IsAny<QueryDefinition>(), null, null))
            .Returns(mockIterator.Object);

        // Act & Assert
        await _sut.Invoking(x => x.UpdatePopularityScoreAsync(memeId, 1))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Meme with ID {memeId} not found");
    }
}
