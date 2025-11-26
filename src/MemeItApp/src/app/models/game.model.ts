export interface CreateGameRequest {
  password?: string;
}

export interface JoinGameRequest {
  gameCode: string;
  password?: string;
}

export interface GameResponse {
  gameCode: string;
  state: string;
  createdAt: string;
  players: Player[];
  rounds: GameRound[];
  isAdmin: boolean;
  currentRoundInfo?: CurrentRoundInfo;
  playerSubmission?: PlayerSubmission;
}

export interface Player {
  playerId: string;
  displayName: string;
  isReady: boolean;
}

export interface GameRound {
  roundNumber: number;
  submissionCount: number;
}

export interface CurrentRoundInfo {
  roundNumber: number;
  startedAt: string;
  phase: 'Creative' | 'Scoring' | 'Ended';
  creativePhaseEndTime?: string;
}

export interface PlayerSubmission {
  memeTemplateId: string;
  textEntries: TextEntry[];
  submittedAt: string;
}

export interface TextEntry {
  textFieldId: string;
  value: string;
}
