using Azure.Storage.Blobs;
using Bogus;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public class ListMemeTemplatesQueryHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly IQueryHandler<ListMemeTemplatesQuery, ListMemeTemplatesResult> _handler;
    private readonly Faker _faker;

    public ListMemeTemplatesQueryHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobServiceClientMock.Setup(x => x.Uri).Returns(new Uri("https://storageaccount.blob.core.windows.net"));
        _handler = new ListMemeTemplatesQueryHandler(_repositoryMock.Object, _blobServiceClientMock.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithAvailableTemplates_ShouldReturnList()
    {
        // Arrange
        var templates = new List<MemeTemplate>
        {
            MemeTemplate.Create(
                _faker.Lorem.Sentence(),
                "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg",
                new List<TextAreaDefinition>
                {
                    TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
                }
            ),
            MemeTemplate.Create(
                _faker.Lorem.Sentence(),
                "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg",
                new List<TextAreaDefinition>
                {
                    TextAreaDefinition.Create(20, 20, 300, 60, 32, "#FF0000", 3, "#FFFFFF", false)
                }
            )
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var query = new ListMemeTemplatesQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Templates);
        Assert.Equal(2, result.Templates.Count);
        Assert.Equal(templates[0].Id, result.Templates[0].Id);
        Assert.Equal(templates[1].Id, result.Templates[1].Id);
        _repositoryMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoTemplates_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MemeTemplate>());

        var query = new ListMemeTemplatesQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Templates);
        Assert.Empty(result.Templates);
    }
}
