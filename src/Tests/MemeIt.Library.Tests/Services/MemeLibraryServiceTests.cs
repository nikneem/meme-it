using Microsoft.Extensions.Logging;
using Moq;
using MemeIt.Core.Models;
using MemeIt.Library.Abstractions;
using MemeIt.Library.Services;
using MemeIt.Library.Tests.TestData;

namespace MemeIt.Library.Tests.Services;

public class MemeLibraryServiceTests
{
    private readonly Mock<IMemeRepository> _memeRepositoryMock;
    private readonly Mock<IMemeCategoryRepository> _categoryRepositoryMock;
    private readonly Mock<ILogger<MemeLibraryService>> _loggerMock;
    private readonly MemeLibraryService _sut;

    public MemeLibraryServiceTests()
    {
        _memeRepositoryMock = new Mock<IMemeRepository>();
        _categoryRepositoryMock = new Mock<IMemeCategoryRepository>();
        _loggerMock = new Mock<ILogger<MemeLibraryService>>();
        _sut = new MemeLibraryService(_memeRepositoryMock.Object, _categoryRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetRandomMemeForPlayerAsync_WithValidParameters_ReturnsMeme()
    {
        // Arrange
        var playerId = "player-123";
        var categories = new List<string> { "humor", "classic" };
        var excludedMemeIds = new List<string> { "excluded-1" };
        var expectedMeme = MemeTestDataFactory.CreateSampleMeme();

        _categoryRepositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(categories, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MemeTestDataFactory.CreateMultipleCategories().Where(c => categories.Contains(c.Id)).ToList());

        _memeRepositoryMock
            .Setup(x => x.GetRandomMemeAsync(categories, excludedMemeIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMeme);

        // Act
        var result = await _sut.GetRandomMemeForPlayerAsync(playerId, categories, excludedMemeIds);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMeme.Id, result.Id);
        Assert.Equal(expectedMeme.Name, result.Name);
        Assert.Equal(expectedMeme.ImageUrl, result.ImageUrl);
        _memeRepositoryMock.Verify(x => x.GetRandomMemeAsync(categories, excludedMemeIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRandomMemeForPlayerAsync_WithEmptyPlayerId_ThrowsArgumentException()
    {
        // Arrange
        var categories = new List<string> { "humor" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetRandomMemeForPlayerAsync("", categories));
        Assert.Equal("playerId", exception.ParamName);
    }

    [Fact]
    public async Task GetRandomMemeForPlayerAsync_WithEmptyCategories_UsesAllActiveCategories()
    {
        // Arrange
        var playerId = "player-123";
        var emptyCategories = new List<string>();
        var allCategories = MemeTestDataFactory.CreateMultipleCategories().Where(c => c.IsActive).ToList();
        var allCategoryIds = allCategories.Select(c => c.Id).ToList();
        var expectedMeme = MemeTestDataFactory.CreateSampleMeme();

        _categoryRepositoryMock
            .Setup(x => x.GetActiveCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCategories);

        _categoryRepositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(allCategoryIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCategories);

        _memeRepositoryMock
            .Setup(x => x.GetRandomMemeAsync(allCategoryIds, It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMeme);

        // Act
        var result = await _sut.GetRandomMemeForPlayerAsync(playerId, emptyCategories);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMeme.Id, result.Id);
        Assert.Equal(expectedMeme.Name, result.Name);
        Assert.Equal(expectedMeme.ImageUrl, result.ImageUrl);
        _categoryRepositoryMock.Verify(x => x.GetActiveCategoriesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRandomMemeForPlayerAsync_WithInvalidCategories_ReturnsNull()
    {
        // Arrange
        var playerId = "player-123";
        var invalidCategories = new List<string> { "invalid-category" };

        _categoryRepositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(invalidCategories, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MemeCategory>());

        // Act
        var result = await _sut.GetRandomMemeForPlayerAsync(playerId, invalidCategories);

        // Assert
        Assert.Null(result);
        _memeRepositoryMock.Verify(x => x.GetRandomMemeAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetMemeByIdAsync_WithValidId_ReturnsMeme()
    {
        // Arrange
        var memeId = "test-meme-1";
        var expectedMeme = MemeTestDataFactory.CreateSampleMeme(memeId);

        _memeRepositoryMock
            .Setup(x => x.GetMemeByIdAsync(memeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedMeme);

        // Act
        var result = await _sut.GetMemeByIdAsync(memeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedMeme.Id, result.Id);
        Assert.Equal(expectedMeme.Name, result.Name);
        Assert.Equal(expectedMeme.ImageUrl, result.ImageUrl);
    }

    [Fact]
    public async Task GetMemeByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var memeId = "non-existent-meme";

        _memeRepositoryMock
            .Setup(x => x.GetMemeByIdAsync(memeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meme?)null);

        // Act
        var result = await _sut.GetMemeByIdAsync(memeId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMemeByIdAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetMemeByIdAsync(""));
        Assert.Equal("memeId", exception.ParamName);
    }

    [Fact]
    public async Task GetAvailableCategoriesAsync_ReturnsActiveCategories()
    {
        // Arrange
        var expectedCategories = MemeTestDataFactory.CreateMultipleCategories().Where(c => c.IsActive).ToList();

        _categoryRepositoryMock
            .Setup(x => x.GetActiveCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _sut.GetAvailableCategoriesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCategories.Count, result.Count);
        for (int i = 0; i < expectedCategories.Count; i++)
        {
            Assert.Equal(expectedCategories[i].Id, result[i].Id);
            Assert.Equal(expectedCategories[i].Name, result[i].Name);
        }
    }

    [Fact]
    public async Task RecordMemeUsageAsync_WithValidParameters_UpdatesPopularityScore()
    {
        // Arrange
        var memeId = "test-meme-1";
        var playerId = "player-123";
        var expectedNewScore = 15;

        _memeRepositoryMock
            .Setup(x => x.UpdatePopularityScoreAsync(memeId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedNewScore);

        // Act
        await _sut.RecordMemeUsageAsync(memeId, playerId);

        // Assert
        _memeRepositoryMock.Verify(x => x.UpdatePopularityScoreAsync(memeId, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordMemeUsageAsync_WithEmptyMemeId_ThrowsArgumentException()
    {
        // Arrange
        var playerId = "player-123";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.RecordMemeUsageAsync("", playerId));
        Assert.Equal("memeId", exception.ParamName);
    }

    [Fact]
    public async Task RecordMemeUsageAsync_WithEmptyPlayerId_ThrowsArgumentException()
    {
        // Arrange
        var memeId = "test-meme-1";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.RecordMemeUsageAsync(memeId, ""));
        Assert.Equal("playerId", exception.ParamName);
    }

    [Fact]
    public async Task RecordMemeUsageAsync_WithNonExistentMeme_DoesNotThrow()
    {
        // Arrange
        var memeId = "non-existent-meme";
        var playerId = "player-123";

        _memeRepositoryMock
            .Setup(x => x.UpdatePopularityScoreAsync(memeId, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Meme not found"));

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _sut.RecordMemeUsageAsync(memeId, playerId));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ValidateCategoriesAsync_WithValidCategories_ReturnsValidIds()
    {
        // Arrange
        var categories = new List<string> { "humor", "classic" };
        var expectedCategories = MemeTestDataFactory.CreateMultipleCategories()
            .Where(c => categories.Contains(c.Id) && c.IsActive)
            .ToList();

        _categoryRepositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(categories, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCategories);

        // Act
        var result = await _sut.ValidateCategoriesAsync(categories);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedCategories.Count, result.Count);
        var expectedIds = expectedCategories.Select(c => c.Id).ToList();
        Assert.Equal(expectedIds, result);
    }

    [Fact]
    public async Task ValidateCategoriesAsync_WithMixedValidAndInvalidCategories_ReturnsOnlyValid()
    {
        // Arrange
        var categories = new List<string> { "humor", "invalid-category", "classic" };
        var validCategories = MemeTestDataFactory.CreateMultipleCategories()
            .Where(c => c.Id == "humor" || c.Id == "classic")
            .ToList();

        _categoryRepositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(categories, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validCategories);

        // Act
        var result = await _sut.ValidateCategoriesAsync(categories);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("humor", result);
        Assert.Contains("classic", result);
        Assert.DoesNotContain("invalid-category", result);
    }

    [Fact]
    public async Task ValidateCategoriesAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var categories = new List<string>();

        // Act
        var result = await _sut.ValidateCategoriesAsync(categories);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _categoryRepositoryMock.Verify(x => x.GetCategoriesByIdsAsync(It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateCategoriesAsync_WithInactiveCategories_ExcludesInactive()
    {
        // Arrange
        var categories = new List<string> { "humor", "inactive" };
        var allCategories = MemeTestDataFactory.CreateMultipleCategories()
            .Where(c => categories.Contains(c.Id))
            .ToList();

        _categoryRepositoryMock
            .Setup(x => x.GetCategoriesByIdsAsync(categories, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allCategories);

        // Act
        var result = await _sut.ValidateCategoriesAsync(categories);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("humor", result);
        Assert.DoesNotContain("inactive", result);
    }
}
