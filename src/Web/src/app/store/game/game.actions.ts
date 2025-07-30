import { createAction, props } from '@ngrx/store';
import { Game, CreateGameRequest, JoinGameRequest, Player } from './game.models';

// Game Creation Actions
export const createGame = createAction(
  '[Game] Create Game',
  props<{ request: CreateGameRequest }>()
);

export const createGameSuccess = createAction(
  '[Game] Create Game Success',
  props<{ game: Game; player: Player }>()
);

export const createGameFailure = createAction(
  '[Game] Create Game Failure',
  props<{ error: string }>()
);

// Game Joining Actions
export const joinGame = createAction(
  '[Game] Join Game',
  props<{ request: JoinGameRequest }>()
);

export const joinGameSuccess = createAction(
  '[Game] Join Game Success',
  props<{ game: Game; player: Player }>()
);

export const joinGameFailure = createAction(
  '[Game] Join Game Failure',
  props<{ error: string }>()
);

// General Game Actions
export const leaveGame = createAction('[Game] Leave Game');

export const leaveGameSuccess = createAction('[Game] Leave Game Success');

export const clearGameError = createAction('[Game] Clear Game Error');

export const setInLobby = createAction(
  '[Game] Set In Lobby',
  props<{ inLobby: boolean }>()
);

// Game Updates (from SignalR or other real-time updates)
export const gameUpdated = createAction(
  '[Game] Game Updated',
  props<{ game: Game }>()
);

export const playerJoined = createAction(
  '[Game] Player Joined',
  props<{ player: Player }>()
);

export const playerLeft = createAction(
  '[Game] Player Left',
  props<{ playerId: string }>()
);

// Game State Persistence Actions
export const restoreGameState = createAction('[Game] Restore Game State');

export const restoreGameStateSuccess = createAction(
  '[Game] Restore Game State Success',
  props<{ game: Game; player: Player; isInLobby: boolean }>()
);

export const restoreGameStateFailure = createAction('[Game] Restore Game State Failure');

// Game State Verification Actions
export const verifyGameState = createAction(
  '[Game] Verify Game State',
  props<{ gameId: string; playerId: string }>()
);

export const verifyGameStateSuccess = createAction(
  '[Game] Verify Game State Success',
  props<{ game: Game }>()
);

export const verifyGameStateFailure = createAction(
  '[Game] Verify Game State Failure',
  props<{ error: string }>()
);
