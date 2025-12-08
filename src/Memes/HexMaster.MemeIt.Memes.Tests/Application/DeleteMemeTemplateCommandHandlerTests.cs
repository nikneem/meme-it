using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Configuration;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Domains;
using HexMaster.MemeIt.Memes.Domains.ValueObjects;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.DeleteMemeTemplate;
using Microsoft.Extensions.Options;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public class DeleteMemeTemplateCommandHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
    private readonly IOptions<BlobStorageOptions> _options;
    private readonly ICommandHandler<DeleteMemeTemplateCommand, DeleteMemeTemplateResult> _handler;

    public DeleteMemeTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _blobStorageServiceMock = new Mock<IBlobStorageService>();
        _options = Options.Create(new BlobStorageOptions
        {
            UploadContainerName = "upload",
            MemesContainerName = "memes"
        });
        _handler = new DeleteMemeTemplateCommandHandler(
            _repositoryMock.Object,
            _blobStorageServiceMock.Object,
            _options);
    }

    [Fact]
    public async Task HandleAsync_WithExistingTemplate_ShouldDeleteBothDatabaseAndBlob()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var imageUrl = "/memes/test-image.jpg";
        var template = MemeTemplate.Create(
            "Test Template",
            imageUrl,
            800,
            600,
            new[] { TextAreaDefinition.Create(10, 10, 100, 50, 24, "#FFFFFF", 2, "#000000", false) });

        _repositoryMock
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _blobStorageServiceMock
            .Setup(s => s.DeleteBlobAsync("test-image.jpg", "memes", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteMemeTemplateCommand(templateId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        _repositoryMock.Verify(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
        _blobStorageServiceMock.Verify(s => s.DeleteBlobAsync("test-image.jpg", "memes", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentTemplate_ShouldReturnFailure()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MemeTemplate?)null);

        var command = new DeleteMemeTemplateCommand(templateId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _blobStorageServiceMock.Verify(s => s.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.HandleAsync(null!, CancellationToken.None));
    }
}
