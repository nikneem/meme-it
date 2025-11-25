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

public class GetRandomMemeTemplateQueryHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly Mock<BlobServiceClient> _blobServiceClientMock;
    private readonly IQueryHandler<GetRandomMemeTemplateQuery, GetRandomMemeTemplateResult> _handler;
    private readonly Faker _faker;

    public GetRandomMemeTemplateQueryHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobServiceClientMock.Setup(x => x.Uri).Returns(new Uri("https://storageaccount.blob.core.windows.net"));
        _handler = new GetRandomMemeTemplateQueryHandler(_repositoryMock.Object, _blobServiceClientMock.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithAvailableTemplate_ShouldReturnRandomTemplate()
    {
        // Arrange
        var template = MemeTemplate.Create(
            _faker.Lorem.Sentence(),
            "/meme-templates/" + _faker.Random.AlphaNumeric(10) + ".jpg",
            800,
            600,
            new List<TextAreaDefinition>
            {
                TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
            }
        );

        _repositoryMock
            .Setup(r => r.GetRandomAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetRandomMemeTemplateQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Template);
        Assert.Equal(template.Id, result.Template.Id);
        Assert.Equal(template.Title, result.Template.Title);
        _repositoryMock.Verify(r => r.GetRandomAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoAvailableTemplates_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetRandomAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemeTemplate?)null);

        var query = new GetRandomMemeTemplateQuery();

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Template);
    }
}
