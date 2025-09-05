export interface Game {
  id: string;
  gameCode: string;
  name: string;
  status: GameStatus;
  hasPassword: boolean;
  maxPlayers: number;
  currentPlayers: number;
  createdAt: Date;
  host: Player;
  players: Player[];
  settings: GameSettings;
}

export interface Player {
  id: string;
  name: string;
  isHost: boolean;
  isReady: boolean;
  score?: number;
}

// Server broadcast response structure (without playerId)
export interface GameStateBroadcast {
  gameCode: string;
  status: string;
  players: ServerPlayer[];
  isPasswordProtected: boolean;
  settings: ServerGameSettings;
}

export interface ServerPlayer {
  id: string;
  name: string;
  isReady: boolean;
}

export interface ServerGameSettings {
  maxPlayers: number;
  numberOfRounds: number;
  category: string;
}

export interface GameSettings {
  maxPlayers: number;
  timePerRound: number;
  totalRounds: number;
  allowsSpectators: boolean;
}

export enum GameStatus {
  Waiting = 'waiting',
  InProgress = 'in-progress',
  Active = 'active',
  Finished = 'finished',
  Cancelled = 'cancelled'
}

export interface CreateGameRequest {
  playerName: string;
  password?: string;
}

export interface JoinGameRequest {
  gameCode: string;
  playerName: string;
  password?: string;
}

export interface GameState {
  currentGame: Game | null;
  isLoading: boolean;
  error: string | null;
  isInLobby: boolean;
}

export const initialGameState: GameState = {
  currentGame: null,
  isLoading: false,
  error: null,
  isInLobby: false
};
