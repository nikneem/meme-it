import { createReducer, on } from '@ngrx/store';
import { PlayerState, initialPlayerState } from './player.models';
import * as PlayerActions from './player.actions';

export const playerReducer = createReducer(
  initialPlayerState,

  // Set Current Player
  on(PlayerActions.setCurrentPlayer, (state, { player }) => ({
    ...state,
    currentPlayer: player,
    isLoading: false,
    error: null
  })),

  // Clear Current Player
  on(PlayerActions.clearCurrentPlayer, (state) => ({
    ...state,
    currentPlayer: null,
    isLoading: false,
    error: null
  })),

  // Update Player Ready Status
  on(PlayerActions.updatePlayerReadyStatus, (state, { isReady }) => ({
    ...state,
    currentPlayer: state.currentPlayer ? {
      ...state.currentPlayer,
      isReady
    } : null
  })),

  // Update Player Host Status
  on(PlayerActions.updatePlayerHostStatus, (state, { isHost }) => ({
    ...state,
    currentPlayer: state.currentPlayer ? {
      ...state.currentPlayer,
      isHost
    } : null
  }))
);
