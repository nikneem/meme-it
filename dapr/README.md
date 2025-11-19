# DAPR Configuration

This project uses DAPR for state management with Redis as the backend store.

## Prerequisites

1. Install DAPR CLI: https://docs.dapr.io/getting-started/install-dapr-cli/
2. Initialize DAPR: `dapr init`

## Components

### State Store (`statestore.yaml`)
- **Type**: `state.redis`
- **Backend**: Redis (uses DAPR's default Redis container)
- **Configuration**: Connects to `localhost:6379`

### Pub/Sub (`pubsub.yaml`)
- **Type**: `pubsub.redis`  
- **Backend**: Redis (uses DAPR's default Redis container)
- **Configuration**: Connects to `localhost:6379`

## Running the Application

1. **Start the Aspire App Host** (this will start Redis and other dependencies):
   ```bash
   cd src/Api/Aspire/HexMaster.MemeIt.Aspire/HexMaster.MemeIt.Aspire.AppHost
   dotnet run
   ```

2. **Run the API with DAPR** (in a separate terminal):
   ```bash
   cd src/Api/HexMaster.MemeIt.Api
   dapr run --app-id memeit-api --app-port 5000 --dapr-http-port 3500 --components-path ../../dapr/components -- dotnet run
   ```

## Alternative: Using DAPR's Redis Container

If you want to use DAPR's built-in Redis container instead of Aspire's Redis:

1. **Start DAPR Redis**:
   ```bash
   dapr init
   ```

2. **Ensure Redis is running**:
   ```bash
   docker ps | grep redis
   ```

3. **Run the API**:
   ```bash
   cd src/Api/HexMaster.MemeIt.Api
   dapr run --app-id memeit-api --app-port 5000 --dapr-http-port 3500 --components-path ../../dapr/components -- dotnet run
   ```

## Configuration Details

### Redis Connection
- **Host**: `localhost:6379`
- **Password**: None (empty)
- **Database**: Default (0)

The DAPR components are configured to work with both:
- Aspire's Redis container (when running through Aspire)
- DAPR's default Redis container (when running standalone)

Both use the same default port (6379) so they work interchangeably.

## Game State Storage

Game states are stored in Redis with the key pattern: `game-{gameCode}`

Example:
- Game Code: `ABCD1234`
- Redis Key: `game-ABCD1234`
- Value: JSON serialized `GameStateData` object

## Benefits of Redis State Store

1. **Fast Performance**: In-memory storage with microsecond latency
2. **Automatic Serialization**: DAPR handles JSON serialization/deserialization
3. **Scalability**: Redis can handle thousands of concurrent games
4. **Persistence**: Redis can be configured for disk persistence if needed
5. **Clustering**: Can be scaled to Redis cluster for high availability
