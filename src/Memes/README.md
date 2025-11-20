# Memes Service Implementation

## Overview
The Memes service enables server administrators to manage meme base images and allows game players to fetch random meme templates for gameplay. This implementation follows the modular monolith architecture with CQRS, Minimal APIs, and Entity Framework Core with PostgreSQL.

## Architecture

### Project Structure
Following ADR 0002 (Modular Monolith Structure):

```
src/Memes/
├── HexMaster.MemeIt.Memes/                    # Domain + Application logic
│   ├── Domains/
│   │   ├── MemeTemplate.cs                    # Aggregate root
│   │   ├── ValueObjects/
│   │   │   └── TextAreaDefinition.cs          # Value object for text areas
│   ├── Application/
│   │   └── MemeTemplates/                     # CQRS handlers
│   │       ├── CreateMemeTemplateCommandHandler.cs
│   │       ├── UpdateMemeTemplateCommandHandler.cs
│   │       ├── DeleteMemeTemplateCommandHandler.cs
│   │       ├── GetRandomMemeTemplateQueryHandler.cs
│   │       ├── ListMemeTemplatesQueryHandler.cs
│   │       └── GetMemeTemplateByIdQueryHandler.cs
│   └── Repositories/
│       └── IMemeTemplateRepository.cs         # Repository interface
│
├── HexMaster.MemeIt.Memes.Abstractions/       # Contracts & DTOs
│   ├── Application/
│   │   ├── Commands/                          # CQRS base interfaces
│   │   ├── Queries/
│   │   └── MemeTemplates/                     # Command/Query definitions
│   ├── Domains/
│   │   └── MemeTemplateDto.cs
│   └── ValueObjects/
│       └── TextAreaDefinitionDto.cs
│
├── HexMaster.MemeIt.Memes.Data.Postgres/     # PostgreSQL adapter
│   ├── MemesDbContext.cs                     # EF Core DbContext
│   ├── Configurations/
│   │   └── MemeTemplateConfiguration.cs      # Entity configuration
│   ├── PostgresMemeTemplateRepository.cs     # Repository implementation
│   └── ServiceCollectionExtensions.cs        # DI registration
│
├── HexMaster.MemeIt.Memes.Api/               # Minimal API host
│   ├── Program.cs                            # API startup & DI
│   ├── Endpoints/
│   │   └── MemeTemplateEndpoints.cs          # HTTP endpoints
│   └── Requests/
│       ├── CreateMemeTemplateRequest.cs
│       └── UpdateMemeTemplateRequest.cs
│
└── HexMaster.MemeIt.Memes.Tests/             # Unit tests (xUnit, Moq, Bogus)
    ├── Domains/                              # Domain model tests
    └── Application/                          # Handler tests
```

## Domain Model

### MemeTemplate (Aggregate Root)
- **ID**: Guid (unique identifier)
- **Title**: string (template name)
- **ImageUrl**: string (absolute URI to base image)
- **TextAreas**: Collection of `TextAreaDefinition` (at least one required)
- **CreatedAt**: DateTimeOffset
- **UpdatedAt**: DateTimeOffset? (nullable)

**Invariants**:
- Title cannot be empty
- ImageUrl must be a valid absolute URI
- At least one text area must be defined
- Cannot remove the last text area

### TextAreaDefinition (Value Object)
Defines where and how text should be rendered on a meme:
- **Position**: X, Y coordinates (pixels)
- **Dimensions**: Width, Height (pixels)
- **FontSize**: integer (must be > 0)
- **FontColor**: string (hex color, e.g., #FFFFFF)
- **BorderSize**: integer (≥ 0)
- **BorderColor**: string (hex color)
- **IsBold**: boolean

**Validation**:
- Width and Height must be > 0
- FontSize must be > 0
- Colors must be valid hex format (#RGB or #RRGGBB)
- BorderSize cannot be negative

## API Endpoints

### Admin Endpoints (Template Management)
- **POST** `/api/memes/templates` - Create a new meme template
- **PUT** `/api/memes/templates/{id}` - Update an existing template
- **DELETE** `/api/memes/templates/{id}` - Delete a template
- **GET** `/api/memes/templates` - List all templates
- **GET** `/api/memes/templates/{id}` - Get template by ID

### Player Endpoint
- **GET** `/api/memes/random` - Get a random meme template for gameplay

## Database

### PostgreSQL with Entity Framework Core
- **Connection Name**: `memes-db`
- **Tables**:
  - `MemeTemplates` (main table)
  - `MemeTemplateTextAreas` (owned entity table)

### Entity Configuration
- `MemeTemplate` uses private setters and encapsulation
- `TextAreaDefinition` mapped as owned entity collection
- Navigation uses field access mode for proper encapsulation

## Aspire Integration

### AppHost Configuration
```csharp
var postgres = builder.AddPostgres("memes-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();
var memesDatabase = postgres.AddDatabase("memes-db");

var memesApi = builder.AddProject<Projects.HexMaster_MemeIt_Memes_Api>("hexmaster-memeit-memes-api")
    .WithReference(memesDatabase)
    .WaitFor(memesDatabase);
```

### Features
- PostgreSQL container with persistent lifetime
- PgAdmin included for database management
- Automatic connection string configuration via Aspire
- Health checks included via Aspire.Npgsql.EntityFrameworkCore.PostgreSQL

## Design Patterns Applied

### Pragmatic Domain-Driven Design
- **Rich Domain Models**: Business rules enforced in `MemeTemplate` entity
- **Value Objects**: `TextAreaDefinition` represents multi-field concept with validation
- **Aggregate Root**: `MemeTemplate` controls its text areas
- **Domain Events**: (Future enhancement)
- **Repository Pattern**: Abstracts data access

### CQRS (Command Query Responsibility Segregation)
- **Commands**: CreateMemeTemplate, UpdateMemeTemplate, DeleteMemeTemplate
- **Queries**: GetRandomMemeTemplate, ListMemeTemplates, GetMemeTemplateById
- **Handlers**: Separate handlers for each command/query
- **Thin endpoints**: HTTP layer delegates to handlers

### Minimal APIs
- Endpoint organization via `MapMemeTemplateEndpoints()` extension method
- Route grouping under `/api/memes`
- OpenAPI/Swagger documentation included
- Scalar API reference for development

## Testing

### Unit Tests (xUnit)
- **Domain Tests**: `MemeTemplateTests`, `TextAreaDefinitionTests`
- **Application Tests**: `CreateMemeTemplateCommandHandlerTests`
- **Tools**: xUnit, Moq (mocking), Bogus (test data generation), FluentAssertions
- **Coverage Target**: ≥80%

### Test Patterns
```csharp
// Domain testing
[Fact]
public void Create_WithValidData_ShouldCreateTemplate()
{
    var template = MemeTemplate.Create(title, imageUrl, textAreas);
    template.Should().NotBeNull();
    template.Id.Should().NotBeEmpty();
}

// Handler testing with Moq
[Fact]
public async Task HandleAsync_WithValidCommand_ShouldCreateTemplateAndReturnId()
{
    _repositoryMock.Setup(r => r.AddAsync(...)).ReturnsAsync(expectedId);
    var result = await _handler.HandleAsync(command, CancellationToken.None);
    result.Id.Should().Be(expectedId);
}
```

## Database Migrations

### Creating Initial Migration
```powershell
cd src\Memes\HexMaster.MemeIt.Memes.Data.Postgres
dotnet ef migrations add InitialCreate --startup-project ..\HexMaster.MemeIt.Memes.Api
dotnet ef database update --startup-project ..\HexMaster.MemeIt.Memes.Api
```

### Design-Time Factory
`MemesDbContextFactory` provides DbContext for migrations without running the full application.

## Running the Service

### Prerequisites
- .NET 10 SDK
- Docker (for PostgreSQL via Aspire)
- Aspire workload installed

### Start with Aspire
```powershell
cd src\Aspire\HexMaster.MemeIt.Aspire\HexMaster.MemeIt.Aspire.AppHost
dotnet run
```

This will:
1. Start PostgreSQL container
2. Start PgAdmin container
3. Start Memes API
4. Configure YARP gateway routing
5. Open Aspire Dashboard

### Access Points
- **Aspire Dashboard**: http://localhost:15000 (or auto-assigned port)
- **Memes API**: Via gateway at http://localhost:5000/memes/
- **Direct API**: Port assigned by Aspire
- **Swagger/Scalar**: http://[api-host]/scalar/v1 (development only)
- **PgAdmin**: Port assigned by Aspire

## ADRs Compliance

- **ADR 0001**: Targets .NET 10
- **ADR 0002**: Modular monolith structure with clear project separation
- **ADR 0003**: .NET Aspire for orchestration
- **ADR 0004**: CQRS pattern with command/query handlers
- **ADR 0005**: Minimal APIs for HTTP endpoints
- **Pragmatic DDD Design**: Rich domain models, value objects where valuable
- **Unit Testing Recommendation**: xUnit, Moq, Bogus

## Future Enhancements

### Potential Improvements
1. **Caching**: Add caching for random meme template retrieval
2. **Validation**: Implement FluentValidation for request DTOs
3. **Domain Events**: Emit events when templates are created/updated/deleted
4. **Image Upload**: Add endpoint for uploading meme images
5. **Tagging**: Add categories/tags for memes
6. **Pagination**: Add paging to list endpoint
7. **Search**: Add search/filter capabilities
8. **Authorization**: Add admin role requirements for management endpoints
9. **Rate Limiting**: Protect endpoints from abuse
10. **Metrics**: Add application metrics for monitoring

## Notes

### Package Version Warnings
The solution shows warnings about Npgsql.EntityFrameworkCore.PostgreSQL 9.0.3 being used with EF Core 10.0.0. This is expected and safe - the Aspire integration package handles compatibility.

### Repository Pattern
The repository interface lives in the core domain project (`HexMaster.MemeIt.Memes`) rather than abstractions to avoid circular dependencies and properly leverage domain types.

### Value Objects
Following pragmatic DDD, we only created value objects where they add real value:
- ✅ `TextAreaDefinition`: Multi-field concept with interdependent validation
- ❌ No `MemeTemplateId`: Simple Guid, no special behavior needed
- ❌ No primitive wrappers for Title, FontSize, etc.
