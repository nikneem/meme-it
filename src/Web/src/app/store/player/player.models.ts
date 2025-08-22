export interface Player {
  id: string;
  name: string;
  isReady: boolean;
  isHost: boolean;
}

export interface PlayerState {
  currentPlayer: Player | null;
  isLoading: boolean;
  error: string | null;
}

export const initialPlayerState: PlayerState = {
  currentPlayer: null,
  isLoading: false,
  error: null
};
