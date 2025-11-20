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

public class CreateMemeTemplateCommandHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly ICommandHandler<CreateMemeTemplateCommand, CreateMemeTemplateResult> _handler;
    private readonly Faker _faker;

    public CreateMemeTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _handler = new CreateMemeTemplateCommandHandler(_repositoryMock.Object);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateTemplateAndReturnId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MemeTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var command = new CreateMemeTemplateCommand(
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
        Assert.Equal(expectedId, result.Id);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<MemeTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithInvalidData_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateMemeTemplateCommand(
            "", // Invalid title
            _faker.Internet.UrlWithPath(),
            new List<TextAreaDefinitionDto>
            {
                new(10, 10, 200, 50, 24, "#FFFFFF", 2, "#000000", true)
            }
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _handler.HandleAsync(command, CancellationToken.None));
    }
}
