# MemeIt.Games

This project contains the Orleans-based game engine for the MemeIt application. It manages game lobbies, rounds, scoring, and all game-related logic using Microsoft Orleans actors (grains).

## Features

- **Game Management**: Create, join, and manage game sessions with unique 6-character game codes
- **Player Management**: Track player state and activities
- **Round-based Gameplay**: Automatic progression through text entry and scoring phases
- **Real-time State Management**: Orleans-based distributed state management
- **Meme Service**: Random meme selection and management
- **Game Codes**: Easy-to-share 6-character alphanumeric codes for joining games

## Architecture

### Grains

- **GameGrain**: Manages individual game sessions, rounds, and game logic
- **PlayerGrain**: Tracks player state and current game participation
- **MemeServiceGrain**: Provides meme templates for game rounds
- **GameRegistryGrain**: Manages game code to game ID mappings

### Models

- **GameState**: Complete game state including players, rounds, and scores
- **RoundState**: Individual round state with meme assignments and scoring
- **PlayerState**: Player-specific state and activity tracking
- **GameOptions**: Configurable game settings
- **GameRegistryState**: Game code registry state

### Services

- **GameService**: High-level service for game operations
- **GameCodeGenerator**: Utility for generating and validating game codes
- **ServiceCollectionExtensions**: Dependency injection setup

## Game Codes

Game codes are 6-character alphanumeric codes (A-Z, 0-9) in uppercase that make it easy for players to join games. Examples: `ABC123`, `XYZ789`, `DEF456`.

### Features:
- **Unique**: Each game gets a unique code when created
- **Short**: Only 6 characters for easy sharing
- **Alphanumeric**: Letters and numbers only (no special characters)
- **Uppercase**: Always displayed in uppercase for consistency
- **Validated**: Input codes are normalized and validated

## Game Flow

1. **Lobby Phase**: 
   - Player creates game and becomes game master
   - Game receives a unique 6-character code (e.g., "ABC123")
   - Other players join using either game ID or game code
   - Game master can configure options
   - Game starts when minimum players joined

2. **Text Entry Phase** (1 minute):
   - Each player gets a random meme template
   - Players fill in text areas for their meme
   - Round ends when all submit or timer expires

3. **Scoring Phase** (2 minutes):
   - Players view and score each other's memes (1-5 stars)
   - Round ends when all scores submitted or timer expires

4. **Round Progression**:
   - Scores are calculated and added to totals
   - New round starts automatically
   - Game ends after configured number of rounds

## Usage

```csharp
// Register services
services.AddGameServices();

// Create a game (returns both game ID and game code)
var (gameId, gameCode) = await gameService.CreateGameAsync(playerId, playerName, "My Game");
Console.WriteLine($"Game created! Share code: {gameCode}");

// Join a game by game code
await gameService.JoinGameByCodeAsync("ABC123", playerId, playerName);

// Join a game by game ID (traditional way)
await gameService.JoinGameAsync(gameId, playerId, playerName);

// Get game by code
var game = await gameService.GetGameByCodeAsync("ABC123");

// Start the game (game master only)
await gameService.StartGameAsync(gameId, playerId);

// Submit meme text during text entry phase
await gameService.SubmitMemeTextAsync(gameId, playerId, textEntries);

// Submit scores during scoring phase
await gameService.SubmitScoreAsync(gameId, playerId, targetPlayerId, score);
```

## HTTP API Endpoints

- `POST /api/games` - Create new game (returns gameId and gameCode)
- `GET /api/games/{id}` - Get game details by ID
- `GET /api/games/by-code/{code}` - Get game details by code
- `POST /api/games/{id}/join` - Join game by ID
- `POST /api/games/join-by-code` - Join game by code
- `POST /api/games/{id}/start` - Start game (game master only)
- `POST /api/games/{id}/submit-text` - Submit meme text
- `POST /api/games/{id}/submit-score` - Submit player scores
- `GET /api/games/{id}/scores` - Get current scores
- `GET /api/games/{id}/current-round` - Get round details

## Game Code Examples

```http
### Create a game and get the code
POST /api/games
{
  "playerId": "player1",
  "playerName": "Alice",
  "gameName": "Fun Game"
}
Response: { "gameId": "...", "gameCode": "ABC123" }

### Join by game code
POST /api/games/join-by-code
{
  "gameCode": "ABC123",
  "playerId": "player2", 
  "playerName": "Bob"
}

### Get game details by code
GET /api/games/by-code/ABC123
```
