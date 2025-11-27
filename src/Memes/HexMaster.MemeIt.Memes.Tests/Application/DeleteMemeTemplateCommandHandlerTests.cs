using HexMaster.MemeIt.Memes.Abstractions.Application.Commands;
using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.DeleteMemeTemplate;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public class DeleteMemeTemplateCommandHandlerTests
{
    private readonly Mock<IMemeTemplateRepository> _repositoryMock;
    private readonly ICommandHandler<DeleteMemeTemplateCommand, DeleteMemeTemplateResult> _handler;

    public DeleteMemeTemplateCommandHandlerTests()
    {
        _repositoryMock = new Mock<IMemeTemplateRepository>();
        _handler = new DeleteMemeTemplateCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingTemplate_ShouldDeleteAndReturnSuccess()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.ExistsAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteMemeTemplateCommand(templateId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        _repositoryMock.Verify(r => r.ExistsAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(templateId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentTemplate_ShouldReturnFailure()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.ExistsAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteMemeTemplateCommand(templateId);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullCommand_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _handler.HandleAsync(null!, CancellationToken.None));
    }
}
