export interface CreateGameRequest {
  password?: string;
}

export interface JoinGameRequest {
  gameCode: string;
  password?: string;
}

export interface GameResponse {
  gameCode: string;
  createdAt: string;
  status: 'waiting' | 'active' | 'completed';
  players: Player[];
}

export interface Player {
  id: string;
  name: string;
  isHost: boolean;
  joinedAt: string;
}
