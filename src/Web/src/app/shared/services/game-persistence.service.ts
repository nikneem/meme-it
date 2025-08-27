import { Injectable, signal } from '@angular/core';
import { Game, Player } from '../../store/game/game.models';

export interface PersistedGameState {
  gameCode: string;
  playerId: string;
  playerName: string;
  isInLobby: boolean;
  timestamp: number; // To handle expiration
}

export interface RoundTimerState {
  gameCode: string;
  roundNumber: number;
  startTime: number;
  timePerRound: number;
}

@Injectable({
  providedIn: 'root'
})
export class GamePersistenceService {
  private readonly STORAGE_KEY = 'meme-it-game-state';
  private readonly TIMER_STORAGE_KEY = 'meme-it-round-timer';
  private readonly EXPIRATION_TIME = 24 * 60 * 60 * 1000; // 24 hours in milliseconds

  saveGameState(game: Game | null, player: Player | null, isInLobby: boolean): void {
    try {
      if (!game || !player || !isInLobby) {
        this.clearGameState();
        return;
      }

      const gameState: PersistedGameState = {
        gameCode: game.code,
        playerId: player.id,
        playerName: player.name,
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
    return gameState !== null && 
           !!gameState.gameCode && 
           !!gameState.playerId && 
           !!gameState.playerName;
  }

  saveRoundTimerState(timerState: RoundTimerState): void {
    try {
      localStorage.setItem(this.TIMER_STORAGE_KEY, JSON.stringify(timerState));
    } catch (error) {
      console.warn('Failed to save round timer state to localStorage:', error);
    }
  }

  getRoundTimerState(): RoundTimerState | null {
    try {
      const stored = localStorage.getItem(this.TIMER_STORAGE_KEY);
      if (stored) {
        return JSON.parse(stored);
      }
    } catch (error) {
      console.warn('Failed to load round timer state from localStorage:', error);
    }
    return null;
  }

  clearRoundTimerState(): void {
    try {
      localStorage.removeItem(this.TIMER_STORAGE_KEY);
    } catch (error) {
      console.warn('Failed to clear round timer state from localStorage:', error);
    }
  }
}
