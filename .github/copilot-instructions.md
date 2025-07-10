# MemeIt - GitHub Copilot Instructions

## Project Overview
MemeIt is an online multiplayer party game where players create text content for memes and score each other's creations. The game consists of 5 rounds, with the highest-scoring player winning.

## Game Flow
1. Each player is presented with a meme image
2. Players enter text for their assigned meme
3. Once all players submit, everyone scores other players' memes
4. Scores accumulate over 5 rounds
5. Player with highest total score wins

## Technical Architecture

### Technology Stack
- **Language**: C# 13
- **Framework**: .NET 9
- **Orchestration**: .NET Aspire
- **State Management**: Microsoft Orleans
- **Testing**: xUnit


### Project Structure
```
/src/
├── Aspire/                        # .NET Aspire orchestration and service defaults
│   ├── HexMaster.MemeIt.Aspire.AppHost/
│   ├── HexMaster.MemeIt.Aspire.ServiceDefaults/
│   ├── MemeIt.Aspire.AppHost/
│   └── MemeIt.Aspire.ServiceDefaults/
├── Api/                           # Backend API solution folder
│   ├── Meme It.sln                # Solution file
│   ├── HexMaster.MemeIt.Api/      # Main API project (controllers, endpoints, DTOs)
│   │   ├── Endpoints/             # API endpoints (e.g., GameEndpoints.cs)
│   │   └── Properties/
│   ├── HexMaster.MemeIt.Core/     # Core domain logic, interfaces, CQRS, shared models
│   │   ├── Cqrs/                  # CQRS interfaces
│   │   ├── DataTransferObjects/   # Shared DTOs
│   │   └── Properties/
│   ├── HexMaster.MemeIt.Games/    # Game-specific logic, Orleans grains, features
│   │   ├── Abstractions/          # Grain interfaces
│   │   ├── DataTransferObjects/   # Game DTOs
│   │   ├── ExtensionMethods/      # Extension methods
│   │   ├── Features/              # Game features (CreateGame, JoinGame, etc.)
│   │   ├── Grains/                # Orleans grains (GameGrain, GamePlayerGrain, etc.)
│   │   └── ValueObjects/          # Value objects
│   └── (future) HexMaster.MemeIt.Users/ # User management (planned/optional)
├── Web/                           # Angular client application
│   ├── angular.json
│   ├── package.json
│   ├── src/                       # Angular app source code
│   │   ├── app/                   # Angular components, services, modules
│   │   └── index.html, main.ts, etc.
│   └── assets/, public/           # Static assets
```

> **Note:** There is currently no `/src/Tests/` folder; tests may be located alongside projects or added in the future.


## Development Guidelines

### Orleans Integration
- Game state is maintained using Microsoft Orleans
- Use Orleans grains for managing game sessions, player states, and scoring
- Follow Orleans best practices for grain lifecycle management
- Implement proper grain interfaces and state persistence

### API Design
- Follow RESTful principles for HTTP endpoints
- Use proper HTTP status codes
- Implement comprehensive error handling
- Use dependency injection for service registration


### Domain-Driven Design
- **HexMaster.MemeIt.Core**: Shared domain models, CQRS interfaces, DTOs, and core business logic
- **HexMaster.MemeIt.Games**: Game-specific business logic, Orleans grains, features, and game flow management
- **HexMaster.MemeIt.Api**: Web API endpoints, controllers, and HTTP-specific logic
- **HexMaster.MemeIt.Users**: (Planned/optional) User management, authentication, and player profiles


### Testing Strategy
- Write unit tests for business logic in respective projects (test projects may be added in the future)
- Mock Orleans grains and external dependencies
- Test game flow scenarios thoroughly
- Maintain high code coverage for critical game mechanics


### Aspire Configuration
- All Aspire orchestration code belongs in `/src/Aspire/`
- Use Aspire service defaults for common configurations
- Configure service discovery and communication through Aspire
- Implement proper health checks and monitoring
## Client Application (Angular)

The client application is an Angular project located in `/src/Web/`.

- Use Angular best practices for component, service, and module organization
- Communicate with the backend API via HTTP (REST)
- Place Angular components, services, and modules in `/src/Web/src/app/`
- Static assets (images, music, etc.) are in `/src/Web/assets/` and `/src/Web/public/`
- Use TypeScript, SCSS, and Angular CLI conventions

> **Note:** Keep API contracts in sync between backend DTOs and Angular models/services.

## Code Patterns to Follow

### Orleans Grains
```csharp
// Game grain interface
public interface IGameGrain : IGrainWithStringKey
{
    Task<GameState> GetGameStateAsync();
    Task StartNewRoundAsync();
    Task SubmitMemeTextAsync(string playerId, string memeText);
    Task SubmitScoreAsync(string playerId, string targetPlayerId, int score);
}

// Game grain implementation
public class GameGrain : Grain, IGameGrain
{
    private readonly IPersistentState<GameState> _gameState;
    
    public GameGrain([PersistentState("gameState")] IPersistentState<GameState> gameState)
    {
        _gameState = gameState;
    }
}
```

### API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IClusterClient _clusterClient;
    
    public GamesController(IClusterClient clusterClient)
    {
        _clusterClient = clusterClient;
    }
    
    [HttpPost("{gameId}/submit-meme")]
    public async Task<IActionResult> SubmitMeme(string gameId, [FromBody] SubmitMemeRequest request)
    {
        // Implementation
    }
}
```

### Service Registration
```csharp
// In Program.cs or service configuration
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IUserService, UserService>();
```

## Game-Specific Considerations

### Game State Management
- Track current round (1-5)
- Manage player submissions per round
- Calculate and store scores
- Handle game lifecycle (lobby, playing, scoring, finished)

### Player Management
- Unique player identification
- Session management
- Real-time communication for game updates

### Scoring System
- Implement fair scoring mechanism
- Prevent self-scoring
- Track cumulative scores across rounds


## File Organization

### When creating new files:
- **API Controllers/Endpoints**: Place in `HexMaster.MemeIt.Api/Endpoints/` (or `Controllers/` if added)
- **Orleans Grains**: Place in `HexMaster.MemeIt.Games/Grains/`
- **Domain Models/DTOs**: Place in `HexMaster.MemeIt.Core/DataTransferObjects/` or `Models/`
- **CQRS Interfaces**: Place in `HexMaster.MemeIt.Core/Cqrs/`
- **Game Features**: Place in `HexMaster.MemeIt.Games/Features/`
- **Angular Components/Services**: Place in `Web/src/app/`
- **Static Assets**: Place in `Web/assets/` or `Web/public/`
- **Tests**: Place alongside the code or in future test projects


### Naming Conventions
- Use PascalCase for classes, methods, and properties (C#)
- Use camelCase for local variables and parameters (C#)
- Suffix Orleans grains with "Grain" (e.g., `GameGrain`, `GamePlayerGrain`)
- Suffix interfaces with "I" prefix (e.g., `IGameGrain`, `IGameService`)
- Use Angular/TypeScript naming conventions for client code

## Performance Considerations
- Use async/await throughout the codebase
- Implement proper caching strategies
- Optimize Orleans grain calls
- Consider real-time communication patterns for game updates

## Security Considerations
- Implement proper authentication and authorization
- Validate all user inputs
- Prevent cheating through proper server-side validation
- Use HTTPS for all communications

## Dependencies to Prefer
- Microsoft.Orleans for state management
- Microsoft.AspNetCore for web APIs
- Microsoft.Extensions.DependencyInjection for DI
- System.Text.Json for JSON serialization
- xUnit for testing

## Querying Microsoft Documentation

You have access to an MCP server called `microsoft.docs.mcp` - this tool allows you to search through Microsoft's latest official documentation, and that information might be more detailed or newer than what's in your training data set. When handling questions around how to work with native Microsoft technologies, such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use this tool for research purposes when dealing with specific / narrowly defined questions that may occur.