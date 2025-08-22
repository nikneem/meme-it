import { createAction, props } from '@ngrx/store';
import { Game, CreateGameRequest, JoinGameRequest, Player } from './game.models';

// Game Creation Actions
export const createGame = createAction(
  '[Game] Create Game',
  props<{ request: CreateGameRequest }>()
);

export const createGameSuccess = createAction(
  '[Game] Create Game Success',
  props<{ game: Game }>()
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
  props<{ game: Game }>()
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

// WebPubSub Connection Actions
export const connectToWebPubSub = createAction(
  '[Game] Connect to WebPubSub',
  props<{ gameCode: string; playerId: string }>()
);

export const connectToWebPubSubSuccess = createAction(
  '[Game] Connect to WebPubSub Success',
  props<{ connectionUrl: string; gameCode: string }>()
);

export const connectToWebPubSubFailure = createAction(
  '[Game] Connect to WebPubSub Failure',
  props<{ error: string }>()
);

export const disconnectFromWebPubSub = createAction(
  '[Game] Disconnect from WebPubSub'
);

export const ensureWebPubSubConnection = createAction(
  '[Game] Ensure WebPubSub Connection'
);

// Real-time message received actions
export const realTimeGameUpdated = createAction(
  '[Game] Real-time Game Updated',
  props<{ game: any }>()
);

export const realTimePlayerJoined = createAction(
  '[Game] Real-time Player Joined',
  props<{ player: Player }>()
);

export const realTimePlayerLeft = createAction(
  '[Game] Real-time Player Left',
  props<{ playerId: string }>()
);

export const realTimePlayerReadyStatusChanged = createAction(
  '[Game] Real-time Player Ready Status Changed',
  props<{ playerId: string; isReady: boolean }>()
);

export const realTimePlayerKicked = createAction(
  '[Game] Real-time Player Kicked',
  props<{ playerId: string }>()
);

export const realTimeGameStarted = createAction(
  '[Game] Real-time Game Started',
  props<{ game: Game }>()
);

// Game State Persistence Actions
export const restoreGameState = createAction('[Game] Restore Game State');

export const restoreGameStateSuccess = createAction(
  '[Game] Restore Game State Success',
  props<{ game: Game; isInLobby: boolean }>()
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

// Server-Based Game State Refresh Actions
export const refreshGameStateFromServer = createAction(
  '[Game] Refresh Game State From Server',
  props<{ gameCode: string; playerId: string; playerName: string }>()
);

export const refreshGameStateFromServerSuccess = createAction(
  '[Game] Refresh Game State From Server Success',
  props<{ game: Game }>()
);

export const refreshGameStateFromServerFailure = createAction(
  '[Game] Refresh Game State From Server Failure',
  props<{ error: string }>()
);

// Player Ready Status Actions
export const setPlayerReadyStatus = createAction(
  '[Game] Set Player Ready Status',
  props<{ isReady: boolean }>()
);

export const setPlayerReadyStatusSuccess = createAction(
  '[Game] Set Player Ready Status Success',
  props<{ game: Game }>()
);

export const setPlayerReadyStatusFailure = createAction(
  '[Game] Set Player Ready Status Failure',
  props<{ error: string }>()
);

// Start Game Actions
export const startGame = createAction('[Game] Start Game');

export const startGameSuccess = createAction(
  '[Game] Start Game Success',
  props<{ game: Game }>()
);

export const startGameFailure = createAction(
  '[Game] Start Game Failure',
  props<{ error: string }>()
);

// Kick Player Actions
export const kickPlayer = createAction(
  '[Game] Kick Player',
  props<{ targetPlayerId: string }>()
);

export const kickPlayerSuccess = createAction(
  '[Game] Kick Player Success',
  props<{ game: Game }>()
);

export const kickPlayerFailure = createAction(
  '[Game] Kick Player Failure',
  props<{ error: string }>()
);
