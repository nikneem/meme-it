import { createAction, props } from '@ngrx/store';
import { Player } from './player.models';

// Player Identity Actions
export const setCurrentPlayer = createAction(
  '[Player] Set Current Player',
  props<{ player: Player }>()
);

export const clearCurrentPlayer = createAction(
  '[Player] Clear Current Player'
);

export const updatePlayerReadyStatus = createAction(
  '[Player] Update Player Ready Status',
  props<{ isReady: boolean }>()
);

export const updatePlayerHostStatus = createAction(
  '[Player] Update Player Host Status',
  props<{ isHost: boolean }>()
);
