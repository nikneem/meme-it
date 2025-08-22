import { createReducer, on } from '@ngrx/store';
import { GameState, initialGameState, GameStatus } from './game.models';
import * as GameActions from './game.actions';

// Helper function to check if the data is a server broadcast format
function isServerBroadcastFormat(data: any): boolean {
  return data && 
         typeof data.gameCode === 'string' && 
         typeof data.status === 'string' && 
         Array.isArray(data.players) &&
         typeof data.isPasswordProtected === 'boolean';
}

// Helper function to map server status to client GameStatus
function mapServerStatusToGameStatus(serverStatus: string): GameStatus {
  switch (serverStatus.toLowerCase()) {
    case 'waiting':
    case 'lobby':
      return GameStatus.Waiting;
    case 'in-progress':
    case 'inprogress':
    case 'active':
      return GameStatus.Active;
    case 'finished':
    case 'completed':
      return GameStatus.Finished;
    case 'cancelled':
      return GameStatus.Cancelled;
    default:
      console.warn(`Unknown server status: ${serverStatus}, defaulting to Waiting`);
      return GameStatus.Waiting;
  }
}

export const gameReducer = createReducer(
  initialGameState,
  
  // Create Game
  on(GameActions.createGame, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),
  
  on(GameActions.createGameSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isLoading: false,
    error: null,
    isInLobby: true
  })),
  
  on(GameActions.createGameFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
    isInLobby: false
  })),
  
  // Join Game
  on(GameActions.joinGame, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),
  
  on(GameActions.joinGameSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isLoading: false,
    error: null,
    isInLobby: true
  })),
  
  on(GameActions.joinGameFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
    isInLobby: false
  })),
  
  // Leave Game
  on(GameActions.leaveGame, (state) => ({
    ...state,
    isLoading: true
  })),
  
  on(GameActions.leaveGameSuccess, (state) => ({
    ...state,
    currentGame: null,
    isLoading: false,
    error: null,
    isInLobby: false
  })),
  
  // General Actions
  on(GameActions.clearGameError, (state) => ({
    ...state,
    error: null
  })),
  
  on(GameActions.setInLobby, (state, { inLobby }) => ({
    ...state,
    isInLobby: inLobby
  })),
  
  // Real-time Updates
  on(GameActions.gameUpdated, (state, { game }) => {
    console.log('gameUpdated: Merging partial game state update', {
      previousGame: state.currentGame,
      updateData: game,
      timestamp: new Date().toISOString()
    });
    
    if (!state.currentGame) {
      console.warn('gameUpdated: No current game state, cannot merge partial update');
      return state;
    }

    // Handle both server broadcast format and full game object format
    const gameData = game as any;
    if (isServerBroadcastFormat(gameData)) {
      // Server broadcast format - merge with existing state
      const updatedGame = {
        ...state.currentGame,
        status: mapServerStatusToGameStatus(gameData.status),
        hasPassword: gameData.isPasswordProtected || state.currentGame.hasPassword,
        players: gameData.players?.map((serverPlayer: any) => ({
          id: serverPlayer.id,
          name: serverPlayer.name,
          isHost: state.currentGame?.host?.id === serverPlayer.id,
          isReady: serverPlayer.isReady,
          score: state.currentGame?.players?.find(p => p.id === serverPlayer.id)?.score
        })) || state.currentGame.players,
        currentPlayers: gameData.players?.length || state.currentGame.currentPlayers,
      };
      
      return {
        ...state,
        currentGame: updatedGame
      };
    } else {
      // Full game object format - use as-is (for API responses)
      return {
        ...state,
        currentGame: game
      };
    }
  }),
  
  on(GameActions.playerJoined, (state, { player }) => {
    if (!state.currentGame) return state;
    
    return {
      ...state,
      currentGame: {
        ...state.currentGame,
        players: [...state.currentGame.players, player],
        currentPlayers: state.currentGame.currentPlayers + 1
      }
    };
  }),
  
  on(GameActions.playerLeft, (state, { playerId }) => {
    if (!state.currentGame) return state;
    
    return {
      ...state,
      currentGame: {
        ...state.currentGame,
        players: state.currentGame.players.filter(p => p.id !== playerId),
        currentPlayers: state.currentGame.currentPlayers - 1
      }
    };
  }),

  // WebPubSub Connection States
  on(GameActions.connectToWebPubSub, (state) => ({
    ...state,
    isLoading: true
  })),

  on(GameActions.connectToWebPubSubSuccess, (state) => ({
    ...state,
    isLoading: false,
    error: null
  })),

  on(GameActions.connectToWebPubSubFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),

  // Real-time Updates
  on(GameActions.realTimeGameUpdated, (state, { game }) => {
    console.log('realTimeGameUpdated: Merging partial game state update', {
      previousGame: state.currentGame,
      updateData: game,
      previousPlayers: state.currentGame?.players?.map((p: any) => ({ id: p.id, name: p.name })),
      newPlayers: game?.players?.map((p: any) => ({ id: p.id, name: p.name })),
      timestamp: new Date().toISOString()
    });
    
    if (!state.currentGame) {
      console.warn('realTimeGameUpdated: No current game state, cannot merge partial update');
      return state;
    }

    // The real-time data should be in server broadcast format
    if (isServerBroadcastFormat(game)) {
      // Server broadcast format - merge with existing state
      const updatedGame = {
        ...state.currentGame,
        status: mapServerStatusToGameStatus(game.status),
        hasPassword: game.isPasswordProtected,
        players: game.players?.map((serverPlayer: any) => ({
          id: serverPlayer.id,
          name: serverPlayer.name,
          isHost: state.currentGame?.host?.id === serverPlayer.id, // Preserve host status
          isReady: serverPlayer.isReady,
          score: state.currentGame?.players?.find(p => p.id === serverPlayer.id)?.score // Preserve existing scores
        })) || state.currentGame.players,
        currentPlayers: game.players?.length || state.currentGame.currentPlayers,
        settings: game.settings ? {
          maxPlayers: game.settings.maxPlayers,
          timePerRound: state.currentGame.settings.timePerRound, // Preserve client-only fields
          totalRounds: game.settings.numberOfRounds,
          allowsSpectators: state.currentGame.settings.allowsSpectators // Preserve client-only fields
        } : state.currentGame.settings
      };
      
      return {
        ...state,
        currentGame: updatedGame
      };
    } else {
      console.warn('realTimeGameUpdated: Received data is not in expected server broadcast format', game);
      return state;
    }
  }),

  on(GameActions.realTimePlayerJoined, (state, { player }) => {
    if (!state.currentGame) {
      console.warn('realTimePlayerJoined: No current game state, ignoring player join');
      return state;
    }
    
    // Validate player data
    if (!player || !player.id) {
      console.error('realTimePlayerJoined: Invalid player data received:', player);
      return state;
    }
    
    // Check if player already exists to avoid duplicates (robust check)
    const playerExists = state.currentGame.players.some(p => 
      p && p.id && p.id === player.id
    );
    
    if (playerExists) {
      console.log('realTimePlayerJoined: Player already exists, not adding duplicate:', {
        playerId: player.id,
        playerName: player.name,
        existingPlayers: state.currentGame.players.map(p => ({ id: p.id, name: p.name }))
      });
      return state;
    }
    
    console.log('realTimePlayerJoined: Adding new player:', {
      playerId: player.id,
      playerName: player.name,
      currentPlayerCount: state.currentGame.players.length
    });
    
    return {
      ...state,
      currentGame: {
        ...state.currentGame,
        players: [...state.currentGame.players, player],
        currentPlayers: state.currentGame.currentPlayers + 1
      }
    };
  }),

  on(GameActions.realTimePlayerLeft, (state, { playerId }) => {
    if (!state.currentGame) return state;
    
    return {
      ...state,
      currentGame: {
        ...state.currentGame,
        players: state.currentGame.players.filter(p => p.id !== playerId),
        currentPlayers: state.currentGame.currentPlayers - 1
      }
    };
  }),

  on(GameActions.realTimePlayerReadyStatusChanged, (state, { playerId, isReady }) => {
    if (!state.currentGame) return state;
    
    return {
      ...state,
      currentGame: {
        ...state.currentGame,
        players: state.currentGame.players.map(player =>
          player.id === playerId ? { ...player, isReady } : player
        )
      }
    };
  }),

  on(GameActions.realTimePlayerKicked, (state, { playerId }) => {
    if (!state.currentGame) return state;
    
    return {
      ...state,
      currentGame: {
        ...state.currentGame,
        players: state.currentGame.players.filter(p => p.id !== playerId),
        currentPlayers: state.currentGame.currentPlayers - 1
      }
    };
  }),

  on(GameActions.realTimeGameStarted, (state, { game }) => ({
    ...state,
    currentGame: game
  })),

  // Game State Persistence
  on(GameActions.restoreGameState, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(GameActions.restoreGameStateSuccess, (state, { game, isInLobby }) => ({
    ...state,
    currentGame: game,
    isInLobby,
    isLoading: false,
    error: null
  })),

  on(GameActions.restoreGameStateFailure, (state) => ({
    ...state,
    isLoading: false,
    error: null,
    isInLobby: false
  })),

  // Game State Verification
  on(GameActions.verifyGameState, (state) => ({
    ...state,
    isLoading: true
  })),

  on(GameActions.verifyGameStateSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isLoading: false,
    error: null
  })),

  on(GameActions.verifyGameStateFailure, (state, { error }) => ({
    ...state,
    currentGame: null,
    isInLobby: false,
    isLoading: false,
    error
  })),

  // Server-Based Game State Refresh
  on(GameActions.refreshGameStateFromServer, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(GameActions.refreshGameStateFromServerSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isInLobby: true,
    isLoading: false,
    error: null
  })),

  on(GameActions.refreshGameStateFromServerFailure, (state, { error }) => ({
    ...state,
    currentGame: null,
    isInLobby: false,
    isLoading: false,
    error
  })),

  // Player Ready Status
  on(GameActions.setPlayerReadyStatus, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(GameActions.setPlayerReadyStatusSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isLoading: false,
    error: null
  })),

  on(GameActions.setPlayerReadyStatusFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),

  // Start Game
  on(GameActions.startGame, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(GameActions.startGameSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isLoading: false,
    error: null
  })),

  on(GameActions.startGameFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  })),

  // Kick Player
  on(GameActions.kickPlayer, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(GameActions.kickPlayerSuccess, (state, { game }) => ({
    ...state,
    currentGame: game,
    isLoading: false,
    error: null
  })),

  on(GameActions.kickPlayerFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error
  }))
);
