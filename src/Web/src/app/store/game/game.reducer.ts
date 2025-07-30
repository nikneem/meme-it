import { createReducer, on } from '@ngrx/store';
import { GameState, initialGameState } from './game.models';
import * as GameActions from './game.actions';

export const gameReducer = createReducer(
  initialGameState,
  
  // Create Game
  on(GameActions.createGame, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),
  
  on(GameActions.createGameSuccess, (state, { game, player }) => ({
    ...state,
    currentGame: game,
    currentPlayer: player,
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
  
  on(GameActions.joinGameSuccess, (state, { game, player }) => ({
    ...state,
    currentGame: game,
    currentPlayer: player,
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
    currentPlayer: null,
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
  on(GameActions.gameUpdated, (state, { game }) => ({
    ...state,
    currentGame: game
  })),
  
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

  // Game State Persistence
  on(GameActions.restoreGameState, (state) => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(GameActions.restoreGameStateSuccess, (state, { game, player, isInLobby }) => ({
    ...state,
    currentGame: game,
    currentPlayer: player,
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
    currentPlayer: null,
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

  on(GameActions.refreshGameStateFromServerSuccess, (state, { game, player }) => ({
    ...state,
    currentGame: game,
    currentPlayer: player,
    isInLobby: true,
    isLoading: false,
    error: null
  })),

  on(GameActions.refreshGameStateFromServerFailure, (state, { error }) => ({
    ...state,
    currentGame: null,
    currentPlayer: null,
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

  on(GameActions.setPlayerReadyStatusSuccess, (state, { game, player }) => ({
    ...state,
    currentGame: game,
    currentPlayer: player,
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
