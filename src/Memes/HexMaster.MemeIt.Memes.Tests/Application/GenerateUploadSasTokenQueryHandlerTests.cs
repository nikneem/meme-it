using HexMaster.MemeIt.Memes.Abstractions.Application.MemeTemplates;
using HexMaster.MemeIt.Memes.Abstractions.Services;
using HexMaster.MemeIt.Memes.Application.MemeTemplates.GenerateUploadSasToken;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Application;

public sealed class GenerateUploadSasTokenQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldGenerateTokenWithBlobName()
    {
        // Arrange
        var blobUrl = "https://storage.blob.core.windows.net/meme-templates/20251127150000-12345678.png";
        var sasToken = "sv=2021-06-08&st=2025-11-27T15:00:00Z";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var mockBlobService = new Mock<IBlobStorageService>();
        mockBlobService
            .Setup(s => s.GenerateUploadSasTokenAsync(
                It.IsAny<string>(),
                60,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((blobUrl, sasToken, expiresAt));

        var handler = new GenerateUploadSasTokenQueryHandler(mockBlobService.Object);
        var query = new GenerateUploadSasTokenQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(blobUrl, result.BlobUrl);
        Assert.Equal(sasToken, result.SasToken);
        Assert.Equal(expiresAt, result.ExpiresAt);
        Assert.Equal("https://storage.blob.core.windows.net/meme-templates", result.ContainerUrl);

        mockBlobService.Verify(s => s.GenerateUploadSasTokenAsync(
            It.Is<string>(name => name.EndsWith(".png") && name.Contains("-")),
            60,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldGenerateUniqueBlobNames()
    {
        // Arrange
        var capturedBlobNames = new List<string>();
        var mockBlobService = new Mock<IBlobStorageService>();
        mockBlobService
            .Setup(s => s.GenerateUploadSasTokenAsync(
                It.IsAny<string>(),
                60,
                It.IsAny<CancellationToken>()))
            .Callback<string, int, CancellationToken>((name, _, _) => capturedBlobNames.Add(name))
            .ReturnsAsync(("https://storage.blob.core.windows.net/meme-templates/test.png", "token", DateTimeOffset.UtcNow.AddHours(1)));

        var handler = new GenerateUploadSasTokenQueryHandler(mockBlobService.Object);

        // Act
        await handler.HandleAsync(new GenerateUploadSasTokenQuery());
        await handler.HandleAsync(new GenerateUploadSasTokenQuery());

        // Assert
        Assert.Equal(2, capturedBlobNames.Count);
        Assert.NotEqual(capturedBlobNames[0], capturedBlobNames[1]);
    }
}
