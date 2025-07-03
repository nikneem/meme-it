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
├── Aspire/                 # .NET Aspire orchestration
│   ├── MemeIt.Aspire.AppHost/
│   └── MemeIt.Aspire.ServiceDefaults/
├── Api/                    # Backend API services
│   ├── MemeIt.Api/        # Main API project
│   ├── MemeIt.Core/       # Core domain logic
│   ├── MemeIt.Games/      # Game-specific logic
│   └── MemeIt.Users/      # User management
└── Tests/                  # Unit and integration tests
    ├── MemeIt.Core.Tests/
    ├── MemeIt.Games.Tests/
    └── MemeIt.Users.Tests/
```

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
- **MemeIt.Core**: Contains shared domain models, interfaces, and core business logic
- **MemeIt.Games**: Game-specific business logic, Orleans grains, and game flow management
- **MemeIt.Users**: User management, authentication, and player profiles
- **MemeIt.Api**: Web API controllers, DTOs, and HTTP-specific logic

### Testing Strategy
- Write unit tests for business logic in respective test projects
- Mock Orleans grains and external dependencies
- Test game flow scenarios thoroughly
- Maintain high code coverage for critical game mechanics

### Aspire Configuration
- All Aspire orchestration code belongs in `/src/Aspire/`
- Use Aspire service defaults for common configurations
- Configure service discovery and communication through Aspire
- Implement proper health checks and monitoring

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
- **Controllers**: Place in `MemeIt.Api/Controllers/`
- **Orleans Grains**: Place in `MemeIt.Games/Grains/`
- **Domain Models**: Place in `MemeIt.Core/Models/`
- **Services**: Place in appropriate domain project (`MemeIt.Games/Services/`, etc.)
- **Tests**: Mirror the source structure in corresponding test projects

### Naming Conventions
- Use PascalCase for classes, methods, and properties
- Use camelCase for local variables and parameters
- Suffix Orleans grains with "Grain" (e.g., `GameGrain`, `PlayerGrain`)
- Suffix interfaces with "I" prefix (e.g., `IGameGrain`, `IGameService`)

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

You have access to an MCP server called `microsoft.docs.mcp` - this tool allows you to search through Microsoft's latest official documentation, and that information might be more detailed or newer than what's in your training data set.

When handling questions around how to work with native Microsoft technologies, such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use this tool for research purposes when dealing with specific / narrowly defined questions that may occur.