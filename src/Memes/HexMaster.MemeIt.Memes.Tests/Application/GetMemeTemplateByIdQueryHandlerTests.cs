using Bogus;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Application.Queries;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public class GetMemeTemplateByIdQueryHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly IQueryHandler<GetMemeTemplateByIdQuery, GetMemeTemplateByIdResult> _handler;
    private readonly Faker _faker;

    public GetMemeTemplateByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _handler = new GetMemeTemplateByIdQueryHandler(_repositoryMock.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithExistingTemplate_ShouldReturnTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = MemeTemplate.Create(
            _faker.Lorem.Sentence(),
            _faker.Internet.UrlWithPath(),
            new List<TextAreaDefinition>
            {
                TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
            }
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var query = new GetMemeTemplateByIdQuery(templateId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Template);
        Assert.Equal(template.Id, result.Template.Id);
        Assert.Equal(template.Title, result.Template.Title);
        _repositoryMock.Verify(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentTemplate_ShouldReturnNull()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemeTemplate?)null);

        var query = new GetMemeTemplateByIdQuery(templateId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Template);
    }
}
