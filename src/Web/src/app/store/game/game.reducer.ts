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
  })
);
