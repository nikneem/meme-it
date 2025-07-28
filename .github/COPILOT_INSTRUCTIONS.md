# GitHub Copilot Instructions for MemeIt

## Project Overview
MemeIt is an online multiplayer party game written in C# using the latest language features and targeting .NET 9. Game state is managed with Microsoft Orleans, but not all services are required to use Orleans—choose the best tool for each job. The project is designed for modern, scalable, and maintainable development.

## Technology Stack
- **Language:** C# (latest features)
- **Framework:** .NET 9
- **Orchestration:** .NET Aspire
- **State Management:** Microsoft Orleans (for game state)
- **Testing:** xUnit (with plain .NET Assert statements)

## Running the Project Locally
To run the full application locally, use the Aspire App Host. This will start all required services and configure inter-service communication automatically:

```powershell
dotnet run --project "src/Aspire/MemeIt.Aspire/MemeIt.Aspire.AppHost"
```

## API Design
- Prefer minimal APIs where possible for new endpoints.
- Use RESTful conventions and proper HTTP status codes.
- Implement comprehensive error handling.
- Use dependency injection for all services.

## File Organization
- **/src/Aspire/**: .NET Aspire orchestration and service defaults
- **/src/Api/HexMaster.MemeIt.Api/**: Main API project (endpoints, controllers, DTOs)
  - **Endpoints/**: API endpoints (prefer minimal APIs)
- **/src/Api/HexMaster.MemeIt.Core/**: Core domain logic, CQRS interfaces, shared models/DTOs
- **/src/Api/HexMaster.MemeIt.Games/**: Game logic, Orleans grains, features, value objects
  - **Grains/**: Orleans grain implementations
  - **Abstractions/Grains/**: Grain interfaces
  - **Features/**: Game features (e.g., CreateGame, JoinGame)
  - **DataTransferObjects/**: Game-specific DTOs
- **/src/Api/HexMaster.MemeIt.Games.Tests/**: Unit tests (xUnit)
- **/src/Web/**: Angular client application
  - **src/app/**: Angular components, services, modules
  - **assets/**, **public/**: Static assets

## Orleans Usage
- Use Orleans grains for game sessions, player state, and scoring.
- Not all services must use Orleans—choose the best tool for each job.
- Follow Orleans best practices for grain lifecycle and state persistence.

## Testing
- Write unit tests using xUnit.
- Use plain .NET Assert statements for assertions.
- Mock Orleans grains and external dependencies as needed.

## Performance
- Use async/await throughout the codebase.
- Optimize Orleans grain calls and avoid unnecessary state reads/writes.
- Implement caching where appropriate.
- Minimize API response times and payload sizes.

## Security
- Validate all user input on the server side.
- Implement authentication and authorization for sensitive endpoints.
- Prevent cheating by validating all game actions server-side.
- Use HTTPS for all communications.

## Documentation and Research
- Frequently consult the `microsoft.docs.mcp` MCP server for the latest official documentation on .NET, Orleans, Aspire, and related technologies.
- Prefer up-to-date, official guidance for all framework and library usage.

---

> **Note:** Keep API contracts in sync between backend DTOs and Angular models/services. Follow project structure and naming conventions as established in the codebase.
