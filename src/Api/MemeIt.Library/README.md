# MemeIt.Library

A comprehensive .NET library for managing memes in the MemeIt application. This library provides functionality for storing, retrieving, and managing memes with their associated text areas and categories using Azure CosmosDB as the data store.

## Features

- **Meme Management**: Create, read, update, and delete memes with associated metadata
- **Category System**: Organize memes into configurable categories
- **Text Areas**: Define positioned text overlays for meme generation
- **Random Selection**: Get random memes based on categories and exclusion rules
- **Popularity Tracking**: Track meme usage and popularity scores
- **CosmosDB Integration**: Optimized for Azure CosmosDB with proper partitioning
- **Comprehensive Testing**: Over 75% code coverage with unit tests

## Architecture

The library follows clean architecture principles with the following structure:

```
MemeIt.Library/
├── Abstractions/           # Interfaces and contracts
├── Services/              # Business logic implementations
├── Infrastructure/        # Data access and external concerns
│   ├── Models/           # CosmosDB document models
│   ├── Repositories/     # Data access implementations
│   └── Configuration/    # Configuration options
└── Extensions/           # Dependency injection setup
```

## Domain Models

### Meme
- **Id**: Unique identifier
- **Name**: Display name of the meme
- **ImageUrl**: URL to the meme image
- **TextAreas**: Collection of positioned text areas
- **Categories**: List of category identifiers
- **Tags**: Additional searchable tags
- **Dimensions**: Width and height in pixels
- **Metadata**: Creation date, modification date, active status
- **Popularity**: Usage tracking score

### TextArea
- **Position**: X, Y coordinates and dimensions
- **Font Settings**: Size, color, alignment
- **Stroke**: Outline configuration
- **Constraints**: Maximum character limits

### MemeCategory
- **Id**: Unique identifier
- **Name**: Display name
- **Metadata**: Description, display order, active status
- **Styling**: Color and icon for UI

## Installation

Add the library to your project:

```xml
<ProjectReference Include="path/to/MemeIt.Library/MemeIt.Library.csproj" />
```

## Configuration

Configure the library in your `appsettings.json`:

```json
{
  "CosmosDb": {
    "ConnectionString": "your-cosmos-connection-string",
    "DatabaseName": "memeit-database",
    "MemesContainerName": "memes",
    "CategoriesContainerName": "categories",
    "CreateIfNotExists": true,
    "MemesContainerThroughput": 400,
    "CategoriesContainerThroughput": 400
  }
}
```

Register services in your DI container:

```csharp
using MemeIt.Library.Extensions;

// In Program.cs or Startup.cs
services.AddMemeLibrary(configuration);

// Initialize database (optional, only if CreateIfNotExists is true)
await serviceProvider.InitializeMemeLibraryDatabaseAsync();
```

## Usage Examples

### Basic Service Usage

```csharp
public class GameController : ControllerBase
{
    private readonly IMemeLibraryService _memeLibraryService;

    public GameController(IMemeLibraryService memeLibraryService)
    {
        _memeLibraryService = memeLibraryService;
    }

    [HttpGet("random-meme")]
    public async Task<ActionResult<Meme>> GetRandomMeme(
        [FromQuery] string playerId,
        [FromQuery] string[] categories = null,
        [FromQuery] string[] excludeIds = null)
    {
        var meme = await _memeLibraryService.GetRandomMemeForPlayerAsync(
            playerId, 
            categories ?? Array.Empty<string>(), 
            excludeIds ?? Array.Empty<string>());

        if (meme == null)
        {
            return NotFound("No memes available");
        }

        // Record usage for popularity tracking
        await _memeLibraryService.RecordMemeUsageAsync(meme.Id, playerId);

        return Ok(meme);
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<MemeCategory>>> GetCategories()
    {
        var categories = await _memeLibraryService.GetAvailableCategoriesAsync();
        return Ok(categories);
    }
}
```

### Direct Repository Usage

```csharp
public class MemeManagementService
{
    private readonly IMemeRepository _memeRepository;
    private readonly IMemeCategoryRepository _categoryRepository;

    public MemeManagementService(
        IMemeRepository memeRepository, 
        IMemeCategoryRepository categoryRepository)
    {
        _memeRepository = memeRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Meme> CreateMemeAsync(CreateMemeRequest request)
    {
        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Categories = request.Categories,
            TextAreas = request.TextAreas.Select(ta => new TextArea
            {
                Id = Guid.NewGuid().ToString(),
                X = ta.X,
                Y = ta.Y,
                Width = ta.Width,
                Height = ta.Height,
                FontSize = ta.FontSize,
                Alignment = ta.Alignment
            }).ToList(),
            Width = request.Width,
            Height = request.Height,
            IsActive = true
        };

        return await _memeRepository.CreateMemeAsync(meme);
    }
}
```

## Testing

The library includes comprehensive unit tests with over 75% code coverage:

```bash
cd src/Tests/MemeIt.Library.Tests
dotnet test --collect:"XPlat Code Coverage"
```

Test categories:
- **Domain Model Tests**: Validation of core models and business rules
- **Service Tests**: Business logic and service layer functionality
- **Repository Tests**: Data access layer with mocked CosmosDB
- **Integration Tests**: Configuration and dependency injection
- **Document Mapping Tests**: CosmosDB document serialization

## Performance Considerations

- **Partitioning**: Memes are partitioned by primary category for optimal distribution
- **Indexing**: Optimized indexes for category, active status, and type queries
- **Connection Pooling**: Singleton CosmosClient with connection pooling
- **Retry Logic**: Built-in retry mechanism for transient failures
- **Caching**: Repository-level caching can be added for frequently accessed data

## Error Handling

The library includes comprehensive error handling:
- **Validation**: Input validation with descriptive error messages
- **Logging**: Structured logging throughout all operations
- **Resilience**: Retry policies for transient failures
- **Graceful Degradation**: Non-critical operations (like popularity tracking) don't fail the main flow

## Contributing

1. Follow the existing code style and patterns
2. Add unit tests for new functionality
3. Maintain minimum 80% code coverage
4. Update documentation for API changes
5. Use semantic versioning for releases

## License

This library is part of the MemeIt application and follows the same licensing terms.
