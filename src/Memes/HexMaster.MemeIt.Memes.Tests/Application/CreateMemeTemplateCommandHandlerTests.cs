using Bogus;
using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Configuration;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Abstractions.ValueObjects;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.CreateMemeTemplate;
using HexMaster.MemeIt.Memes.Domains;
using Microsoft.Extensions.Options;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public class CreateMemeTemplateCommandHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
    private readonly IOptions<BlobStorageOptions> _options;
    private readonly ICommandHandler<CreateMemeTemplateCommand, CreateMemeTemplateResult> _handler;
    private readonly Faker _faker;

    public CreateMemeTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _blobStorageServiceMock = new Mock<IBlobStorageService>();
        _options = Options.Create(new BlobStorageOptions
        {
            UploadContainerName = "upload",
            MemesContainerName = "memes"
        });
        _handler = new CreateMemeTemplateCommandHandler(_repositoryMock.Object, _blobStorageServiceMock.Object, _options);
        _faker = new Faker();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldCreateTemplateAndReturnId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var blobName = _faker.Random.AlphaNumeric(10) + ".jpg";
        var destinationBlobUrl = $"https://storage.blob.core.windows.net/memes/{blobName}";

        _blobStorageServiceMock
            .Setup(b => b.MoveBlobAsync(
                It.IsAny<string>(),
                "upload",
                "memes",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationBlobUrl);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MemeTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        var command = new CreateMemeTemplateCommand(
            _faker.Lorem.Sentence(),
            "/upload/" + blobName,
            800,
            600,
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
        _blobStorageServiceMock.Verify(b => b.MoveBlobAsync(
            It.IsAny<string>(),
            "upload",
            "memes",
            It.IsAny<CancellationToken>()), Times.Once);
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
        var blobName = _faker.Random.AlphaNumeric(10) + ".jpg";
        var destinationBlobUrl = $"https://storage.blob.core.windows.net/memes/{blobName}";

        _blobStorageServiceMock
            .Setup(b => b.MoveBlobAsync(
                It.IsAny<string>(),
                "upload",
                "memes",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationBlobUrl);

        var command = new CreateMemeTemplateCommand(
            "", // Invalid title
            "/upload/" + blobName,
            800,
            600,
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
