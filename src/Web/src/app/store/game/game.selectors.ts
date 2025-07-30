import { createSelector, createFeatureSelector } from '@ngrx/store';
import { GameState } from './game.models';

export const selectGameState = createFeatureSelector<GameState>('game');

export const selectCurrentGame = createSelector(
  selectGameState,
  (state) => state.currentGame
);

export const selectCurrentPlayer = createSelector(
  selectGameState,
  (state) => state.currentPlayer
);

export const selectIsLoading = createSelector(
  selectGameState,
  (state) => state.isLoading
);

export const selectGameError = createSelector(
  selectGameState,
  (state) => state.error
);

export const selectIsInLobby = createSelector(
  selectGameState,
  (state) => state.isInLobby
);

export const selectIsHost = createSelector(
  selectCurrentPlayer,
  (player) => player?.isHost ?? false
);

export const selectCanStartGame = createSelector(
  selectCurrentGame,
  selectIsHost,
  (game, isHost) => {
    if (!game || !isHost) return false;
    return game.players.length >= 2 && game.players.every(p => p.isReady);
  }
);

export const selectGameCode = createSelector(
  selectCurrentGame,
  (game) => game?.code
);

export const selectPlayerCount = createSelector(
  selectCurrentGame,
  (game) => ({
    current: game?.currentPlayers ?? 0,
    max: game?.maxPlayers ?? 0
  })
);
