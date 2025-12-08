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
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenContainerNameIsNull()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync(null!, "test.png"));
    }

    [Fact]
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenBlobNameIsNull()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync("upload", null!));
    }

    [Fact]
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenBlobNameIsEmpty()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync("upload", ""));
    }

    [Fact]
    public async Task GenerateUploadSasTokenAsync_ShouldThrowWhenBlobNameIsWhitespace()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.GenerateUploadSasTokenAsync("upload", "   "));
    }

    [Fact]
    public async Task MoveBlobAsync_ShouldThrowWhenSourceBlobNameIsNull()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.MoveBlobAsync(null!, "upload", "memes"));
    }

    [Fact]
    public async Task MoveBlobAsync_ShouldThrowWhenSourceContainerNameIsNull()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.MoveBlobAsync("test.png", null!, "memes"));
    }

    [Fact]
    public async Task MoveBlobAsync_ShouldThrowWhenDestinationContainerNameIsNull()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var service = new BlobStorageService(mockBlobServiceClient.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.MoveBlobAsync("test.png", "upload", null!));
    }
}
