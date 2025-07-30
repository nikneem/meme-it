export interface Game {
  id: string;
  code: string;
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
  currentPlayer: Player | null;
  isLoading: boolean;
  error: string | null;
  isInLobby: boolean;
}

export const initialGameState: GameState = {
  currentGame: null,
  currentPlayer: null,
  isLoading: false,
  error: null,
  isInLobby: false
};
