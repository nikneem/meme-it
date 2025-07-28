# Meme-IT

Meme-IT is an open source online multiplayer meme creator game. The goal is to create the funniest memes and score the highest points over a series of rounds.

## Game Overview

Players join a game lobby, where they wait until the game master starts the game. The game consists of 5 rounds, and each round has two phases:

### 1. Creation Phase
- Each player is presented with an empty meme image.
- Players enter their own text to make the meme as hilarious as possible.
- When all players have submitted their meme text, or when the creation phase timer ends, the game moves to the next phase.

### 2. Scoring Phase
- All created memes are shown to all players.
- Players rate each meme (except their own) with a 1 to 5 star rating.
- Points are awarded to players based on the number of stars their meme receives.

After scoring, a new round begins. This repeats until all 5 rounds are played.

## Winning

At the end of 5 rounds, all scores are totaled. The player with the highest score wins the game. A new game is then created, and all players return to the lobby to play again.

---

If you can dream it, you can meme it.

---

## Technical Overview

### Technology Stack
- **Backend Language:** C# 13
- **Framework:** .NET 9
- **Orchestration:** .NET Aspire
- **State Management:** Microsoft Orleans (virtual actor model)
- **API:** ASP.NET Core Web API
- **Frontend:** Angular (TypeScript, SCSS)
- **Testing:** xUnit (backend)

### Key Techniques
- Orleans grains for distributed, persistent game and player state
- RESTful API endpoints for game actions and state queries
- Dependency injection for service management
- Domain-driven design for clear separation of concerns
- Real-time game flow and scoring logic

### Folder Structure
```
src/
  Aspire/                  # .NET Aspire orchestration and service defaults
    HexMaster.MemeIt.Aspire.AppHost/
    HexMaster.MemeIt.Aspire.ServiceDefaults/
    MemeIt.Aspire.AppHost/
    MemeIt.Aspire.ServiceDefaults/
  Api/                     # Backend API solution folder
    Meme It.sln            # Solution file
    HexMaster.MemeIt.Api/  # Main API project (controllers, endpoints, DTOs)
      Endpoints/
      Properties/
    HexMaster.MemeIt.Core/ # Core domain logic, interfaces, CQRS, shared models
      Cqrs/
      DataTransferObjects/
      Properties/
    HexMaster.MemeIt.Games/# Game logic, Orleans grains, features
      Abstractions/
      DataTransferObjects/
      ExtensionMethods/
      Features/
      Grains/
      ValueObjects/
    HexMaster.MemeIt.Games.Tests/ # xUnit tests for game logic
    HexMaster.MemeIt.Memes/ # Meme-related logic and DTOs
  Web/                      # Angular client application
    angular.json
    package.json
    src/
      app/                  # Angular components, services, modules
      index.html, main.ts, styles.scss
    assets/, public/        # Static assets
```

---

## Getting Started (Local Development)

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/)
- [Node.js & npm](https://nodejs.org/) (for Angular frontend)

### 1. Clone the Repository
```powershell
git clone https://github.com/nikneem/meme-it.git
cd meme-it
```

### 2. Build and Run the Backend
```powershell
cd src/Api
dotnet build
dotnet run --project "HexMaster.MemeIt.Api/HexMaster.MemeIt.Api.csproj"
```
Or use Aspire orchestration:
```powershell
dotnet run --project "src/Aspire/MemeIt.Aspire/MemeIt.Aspire.AppHost"
```

### 3. Start the Angular Frontend
```powershell
cd ../../Web
npm install
ng serve
```
The Angular app will be available at `http://localhost:4200`.

### 4. Access the Game
- Open your browser and navigate to the frontend URL.
- The frontend communicates with the backend API for all game actions.

---

## Contributing
Pull requests and issues are welcome! Please follow the project structure and coding conventions described above.
