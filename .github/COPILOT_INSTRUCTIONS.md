# GitHub Copilot Instructions for MemeIt

## Project Overview
MemeIt is an online multiplayer party game written in C# using the latest language features and targeting .NET 9. Game state is managed with Microsoft Orleans, but not all services are required to use Orleans—choose the best tool for each job. The project is designed for modern, scalable, and maintainable development.

## Technology Stack
- **Language:** C# (latest features)
- **Framework:** .NET 9
- **Orchestration:** .NET Aspire (manages both backend and frontend services)
- **Frontend:** Angular (TypeScript, SCSS) - orchestrated through Aspire
- **State Management:** Microsoft Orleans (for game state)
- **Testing:** xUnit (with plain .NET Assert statements)

## Code Standards and Best Practices
- **DateTime Usage:** Always use `DateTimeOffset` instead of `DateTime` for all date/time operations. This ensures proper timezone handling and consistency across the application.
- Use async/await throughout the codebase.
- Follow modern C# conventions and use the latest language features.
- Implement proper error handling and validation.

## Running the Project Locally
To run the full application locally, use the Aspire App Host. This will start and orchestrate all required services including the Angular frontend, backend APIs, and databases, and configure inter-service communication automatically:

```powershell
dotnet run --project "src/Api/Aspire/HexMaster.MemeIt.Aspire/HexMaster.MemeIt.Aspire.AppHost"
```

**Important:** Both the backend APIs and the Angular frontend application are fully orchestrated and automatically started by the .NET Aspire host. The frontend will be available at its designated port (typically `http://localhost:4200` or another assigned port). There is no need to start any services manually using `npm start`, `ng serve`, or separate `dotnet run` commands.

### VS Code Tasks
The project includes comprehensive VS Code tasks in `.vscode/tasks.json` for common development operations:
- **`build-all`** (default build): Install frontend deps, build backend, build frontend
- **`start-aspire`**: Start Aspire orchestration with proper background task handling
- **`dev`**: Complete development workflow - build all then start Aspire
- **`run-tests`**: Execute all backend unit tests
- **`clean-all`** / **`rebuild-all`**: Clean and rebuild operations

Use `Ctrl+Shift+P` → "Tasks: Run Task" in VS Code to access these tasks, or `Ctrl+Shift+B` for the default build task.

## Development Environment Guidelines
- **Terminal Usage:** Always reuse existing terminals when possible. Avoid opening new terminal windows unnecessarily.
- **Application Startup:** Use the single Aspire AppHost command to start the entire application stack. All services (backend APIs, Angular frontend, databases, etc.) are orchestrated and managed through .NET Aspire.
- **Frontend Access:** Once Aspire is running, access the Angular application at the port assigned by Aspire orchestration (check the Aspire dashboard for the exact URL).
- **Service Dependencies:** All inter-service communication and port assignments are handled automatically by Aspire. Manual service startup is not required and should be avoided.
- **VS Code Tasks:** Use the predefined VS Code tasks for common operations:
  - `build-all`: Build both backend and frontend (default build task)
  - `start-aspire`: Start the Aspire orchestration
  - `dev`: Build everything and start development environment
  - `run-tests`: Execute unit tests
  - `clean-all` / `rebuild-all`: Clean and rebuild operations

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
