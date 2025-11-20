using Bogus;
using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;
using HexMaster.MemeIt.Memes.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public class UpdateMemeTemplateCommandHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly ICommandHandler<UpdateMemeTemplateCommand, UpdateMemeTemplateResult> _handler;
    private readonly Faker _faker;

    public UpdateMemeTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _handler = new UpdateMemeTemplateCommandHandler(_repositoryMock.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUpdateTemplateAndReturnSuccess()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var existingTemplate = MemeTemplate.Create(
            _faker.Lorem.Sentence(),
            _faker.Internet.UrlWithPath(),
            new List<TextAreaDefinition>
            {
                TextAreaDefinition.Create(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
            }
        );

        _repositoryMock
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTemplate);

        var command = new UpdateMemeTemplateCommand(
            templateId,
            _faker.Lorem.Sentence(),
            _faker.Internet.UrlWithPath(),
            new List<TextAreaDefinitionDto>
            {
                new(20, 20, 300, 60, 32, "#FF0000", 3, "#FFFFFF", false)
            }
        );

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        _repositoryMock.Verify(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MemeTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentTemplate_ShouldReturnFailure()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemeTemplate?)null);

        var command = new UpdateMemeTemplateCommand(
            templateId,
            _faker.Lorem.Sentence(),
            _faker.Internet.UrlWithPath(),
            new List<TextAreaDefinitionDto>
            {
                new(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
            }
        );

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<MemeTemplate>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.HandleAsync(null!, CancellationToken.None));
    }
}
