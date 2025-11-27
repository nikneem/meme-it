using Azure.Storage.Blobs;
using HexMaster.MemeIt.Memes.Services;
using Moq;

namespace HexMaster.MemeIt.Memes.Tests.Services;

public sealed class BlobStorageServiceTests
{
    [Fact]
    public void Constructor_ShouldThrowWhenBlobServiceClientIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BlobStorageService(null!));
    }

    [Fact]
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenBlobNameIsNull()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync(null!));
    }

    [Fact]
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenBlobNameIsEmpty()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync(""));
    }

    [Fact]
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenBlobNameIsWhitespace()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync("   "));
    }
}
