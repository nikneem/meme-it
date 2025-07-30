import { Injectable, signal } from '@angular/core';
import { Game, Player } from '../../store/game/game.models';

export interface PersistedGameState {
  currentGame: Game | null;
  currentPlayer: Player | null;
  isInLobby: boolean;
  timestamp: number; // To handle expiration
}

@Injectable({
  providedIn: 'root'
})
export class GamePersistenceService {
  private readonly STORAGE_KEY = 'meme-it-game-state';
  private readonly EXPIRATION_TIME = 24 * 60 * 60 * 1000; // 24 hours in milliseconds

  saveGameState(game: Game | null, player: Player | null, isInLobby: boolean): void {
    try {
      const gameState: PersistedGameState = {
        currentGame: game,
        currentPlayer: player,
        isInLobby,
        timestamp: Date.now()
      };
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(gameState));
    } catch (error) {
      console.warn('Failed to save game state to localStorage:', error);
    }
  }

  loadGameState(): PersistedGameState | null {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const parsed: PersistedGameState = JSON.parse(stored);
        
        // Check if the stored state is not expired
        if (Date.now() - parsed.timestamp < this.EXPIRATION_TIME) {
          return parsed;
        } else {
          // Clean up expired state
          this.clearGameState();
        }
      }
    } catch (error) {
      console.warn('Failed to load game state from localStorage:', error);
      this.clearGameState();
    }
    return null;
  }

  clearGameState(): void {
    try {
      localStorage.removeItem(this.STORAGE_KEY);
    } catch (error) {
      console.warn('Failed to clear game state from localStorage:', error);
    }
  }

  hasValidGameState(): boolean {
    const gameState = this.loadGameState();
    return gameState !== null && gameState.isInLobby && gameState.currentGame !== null && gameState.currentPlayer !== null;
  }
}
