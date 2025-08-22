import { createFeatureSelector, createSelector } from '@ngrx/store';
import { PlayerState } from './player.models';

export const selectPlayerState = createFeatureSelector<PlayerState>('player');

export const selectCurrentPlayer = createSelector(
  selectPlayerState,
  (state: PlayerState) => state.currentPlayer
);

export const selectCurrentPlayerId = createSelector(
  selectCurrentPlayer,
  (player) => player?.id || null
);

export const selectCurrentPlayerName = createSelector(
  selectCurrentPlayer,
  (player) => player?.name || null
);

export const selectIsCurrentPlayerReady = createSelector(
  selectCurrentPlayer,
  (player) => player?.isReady || false
);

export const selectIsCurrentPlayerHost = createSelector(
  selectCurrentPlayer,
  (player) => player?.isHost || false
);

export const selectPlayerLoading = createSelector(
  selectPlayerState,
  (state: PlayerState) => state.isLoading
);

export const selectPlayerError = createSelector(
  selectPlayerState,
  (state: PlayerState) => state.error
);
