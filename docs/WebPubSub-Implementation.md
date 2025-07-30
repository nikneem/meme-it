# Azure Web PubSub Real-time Integration Implementation

## Overview
This implementation adds real-time communication to the Meme-It game using Azure Web PubSub with native WebSocket connections on the client side.

## Backend Implementation

### 1. Dependencies Added
- `Azure.Messaging.WebPubSub` NuGet package to HexMaster.MemeIt.Api and HexMaster.MemeIt.Core projects

### 2. New Services Created

#### WebPubSubService (HexMaster.MemeIt.Core.Services)
- `IWebPubSubService` interface with methods:
  - `GenerateConnectionUrlAsync()` - Creates authenticated WebSocket connection URLs
  - `BroadcastToGameAsync()` - Sends messages to all players in a game
  - `AddPlayerToGameGroupAsync()` - Adds player to game group
  - `RemovePlayerFromGameGroupAsync()` - Removes player from game group
  - `SendToPlayerAsync()` - Sends messages to specific players

#### Data Transfer Objects
- `WebPubSubConnectionRequest` - Request for connection URL
- `WebPubSubConnectionResponse` - Response with connection details
- `GameUpdateMessage` - Real-time message structure
- `GameUpdateMessageTypes` - Constants for message types

### 3. New API Endpoint
- `POST /games/connection` - Validates player and returns WebSocket connection URL

### 4. Broadcasting Integration
Modified existing endpoints to broadcast real-time updates:
- `JoinGame` - Broadcasts player joined and game updated events
- `LeaveGame` - Broadcasts player left event
- (Ready to add: KickPlayer, SetPlayerReadyStatus, StartGame)

### 5. Configuration
Added to `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "WebPubSub": "Endpoint=https://meme-it.webpubsub.azure.com;AccessKey=YOUR_ACCESS_KEY_HERE;Version=1.0;"
  },
  "WebPubSub": {
    "HubName": "gameHub"
  }
}
```

## Frontend Implementation (Angular)

### 1. New Services Created

#### WebPubSubService
- Handles native WebSocket connections to Azure Web PubSub
- Implements automatic reconnection with exponential backoff
- Provides observables for connection status and incoming messages
- Supports Azure Web PubSub protocol with group joining

#### WebPubSubConnectionService
- Handles HTTP requests to get WebSocket connection URLs from the server
- Validates player credentials before establishing WebSocket connection

### 2. NgRx Store Updates

#### New Actions
- `connectToWebPubSub` - Initiates WebSocket connection
- `connectToWebPubSubSuccess/Failure` - Connection result actions
- `realTimeGameUpdated` - Handles real-time game state updates
- `realTimePlayerJoined/Left` - Handles player join/leave events
- `realTimePlayerReadyStatusChanged` - Handles ready status updates
- `realTimePlayerKicked` - Handles player kick events
- `realTimeGameStarted` - Handles game start events

#### Effects Updates
- Auto-connect to WebPubSub when joining or creating games
- Listen to real-time messages and dispatch appropriate actions
- Handle disconnection when leaving games
- Map real-time events to existing game state actions

#### Reducer Updates
- Handle WebPubSub connection states
- Update game state from real-time messages
- Prevent duplicate player additions
- Handle player kick scenarios (redirect if current player is kicked)

### 3. UI Updates

#### Game Lobby Component
- Added WebSocket connection status indicator
- Real-time updates without page refresh
- Visual feedback for connection state (connected/connecting/disconnected)

## Real-time Events Supported

1. **Player Joined** - When a new player joins the game
2. **Player Left** - When a player leaves the game
3. **Game Updated** - Full game state synchronization
4. **Player Ready Status Changed** - When players mark themselves ready
5. **Player Kicked** - When a player is removed by the host
6. **Game Started** - When the game transitions from lobby to active

## Security Features

1. **Player Validation** - Server validates player exists in game before providing connection URL
2. **JWT Token Authentication** - WebSocket connections use JWT tokens with game-specific permissions
3. **Group Permissions** - Players can only join groups for games they're part of
4. **Token Expiration** - Connection tokens expire after 2 hours

## Connection Flow

1. Player joins/creates game through HTTP API
2. NgRx effect automatically initiates WebPubSub connection
3. Angular service requests connection URL from server
4. Server validates player and generates signed JWT connection URL
5. WebSocket connects to Azure Web PubSub using native JavaScript WebSocket
6. Client joins game-specific group (e.g., "game-ABC123")
7. Real-time messages are received and processed through NgRx store
8. UI updates automatically without page refresh

## Error Handling

1. **Connection Failures** - Automatic reconnection with exponential backoff
2. **Invalid Players** - Server rejects connection requests for non-existent players
3. **Network Issues** - Client shows connection status and attempts reconnection
4. **Token Expiration** - Handled through reconnection mechanism

## Configuration Required

To complete the setup, you need to:

1. Replace `YOUR_ACCESS_KEY_HERE` in `appsettings.Development.json` with your actual Azure Web PubSub access key
2. Ensure your Azure Web PubSub instance is configured with the "gameHub" hub name
3. Configure CORS settings in Azure Web PubSub to allow your client domain

## Testing

1. Start the Aspire AppHost (already running)
2. Start the Angular development server: `npm start`
3. Create or join a game
4. Open the same game in multiple browser windows/tabs
5. Verify real-time updates appear across all connected clients
6. Test various scenarios: join, leave, ready status changes

## Future Enhancements

1. Add broadcasting for remaining endpoints (KickPlayer, SetPlayerReadyStatus, StartGame)
2. Implement player typing indicators
3. Add real-time chat functionality
4. Implement game state recovery for disconnected players
5. Add metrics and monitoring for WebSocket connections
