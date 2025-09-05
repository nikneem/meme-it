import { Injectable } from '@angular/core';

export interface PersistedGameData {
  playerId: string;
  playerName: string;
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
  private readonly GAME_DATA_PREFIX = 'meme-it-game-';
  private readonly TIMER_STORAGE_KEY = 'meme-it-round-timer';
  private readonly EXPIRATION_TIME = 24 * 60 * 60 * 1000; // 24 hours in milliseconds

  /**
   * Store minimal player data for a specific game code
   */
  storePlayerData(gameCode: string, playerId: string, playerName: string): void {
    try {
      console.log('GamePersistenceService.storePlayerData called with:', {
        gameCode,
        playerId,
        playerName
      });

      const gameData: PersistedGameData = {
        playerId,
        playerName,
        timestamp: Date.now()
      };

      const storageKey = this.getGameStorageKey(gameCode);
      console.log('GamePersistenceService: Storing player data:', { storageKey, gameData });
      sessionStorage.setItem(storageKey, JSON.stringify(gameData));
    } catch (error) {
      console.warn('Failed to store player data in sessionStorage:', error);
    }
  }

  /**
   * Get player data for a specific game code
   */
  getPlayerData(gameCode: string): PersistedGameData | null {
    try {
      const storageKey = this.getGameStorageKey(gameCode);
      const stored = sessionStorage.getItem(storageKey);
      console.log('GamePersistenceService.getPlayerData: Raw stored data:', { gameCode, stored });
      
      if (stored) {
        const parsed: PersistedGameData = JSON.parse(stored);
        console.log('GamePersistenceService.getPlayerData: Parsed data:', parsed);
        
        // Check if the stored data is not expired
        if (Date.now() - parsed.timestamp < this.EXPIRATION_TIME) {
          console.log('GamePersistenceService.getPlayerData: Data is valid (not expired)');
          return parsed;
        } else {
          console.log('GamePersistenceService.getPlayerData: Data is expired, clearing');
          this.clearPlayerData(gameCode);
        }
      } else {
        console.log('GamePersistenceService.getPlayerData: No stored data found for game:', gameCode);
      }
    } catch (error) {
      console.warn('Failed to load player data from sessionStorage:', error);
      this.clearPlayerData(gameCode);
    }
    return null;
  }

  /**
   * Check if valid player data exists for a game code
   */
  hasPlayerData(gameCode: string): boolean {
    const playerData = this.getPlayerData(gameCode);
    const isValid = playerData !== null && 
           !!playerData.playerId && 
           !!playerData.playerName;
    console.log('GamePersistenceService.hasPlayerData:', {
      gameCode,
      playerData,
      isValid,
      hasPlayerId: !!playerData?.playerId,
      hasPlayerName: !!playerData?.playerName
    });
    return isValid;
  }

  /**
   * Remove player data for a specific game code
   */
  clearPlayerData(gameCode: string): void {
    try {
      const storageKey = this.getGameStorageKey(gameCode);
      console.log('GamePersistenceService.clearPlayerData: Clearing data for game:', gameCode);
      sessionStorage.removeItem(storageKey);
    } catch (error) {
      console.warn('Failed to clear player data from sessionStorage:', error);
    }
  }

  /**
   * Clear all game data from sessionStorage
   */
  clearAllGameData(): void {
    try {
      // Get all keys that match our game data pattern
      const keysToRemove: string[] = [];
      for (let i = 0; i < sessionStorage.length; i++) {
        const key = sessionStorage.key(i);
        if (key && key.startsWith(this.GAME_DATA_PREFIX)) {
          keysToRemove.push(key);
        }
      }
      
      // Remove all matching keys
      keysToRemove.forEach(key => sessionStorage.removeItem(key));
      console.log('GamePersistenceService.clearAllGameData: Cleared', keysToRemove.length, 'game data entries');
    } catch (error) {
      console.warn('Failed to clear all game data from sessionStorage:', error);
    }
  }

  private getGameStorageKey(gameCode: string): string {
    return `${this.GAME_DATA_PREFIX}${gameCode.toUpperCase()}`;
  }

  // Round timer methods remain the same
  saveRoundTimerState(timerState: RoundTimerState): void {
    try {
      sessionStorage.setItem(this.TIMER_STORAGE_KEY, JSON.stringify(timerState));
    } catch (error) {
      console.warn('Failed to save round timer state to sessionStorage:', error);
    }
  }

  getRoundTimerState(): RoundTimerState | null {
    try {
      const stored = sessionStorage.getItem(this.TIMER_STORAGE_KEY);
      if (stored) {
        return JSON.parse(stored);
      }
    } catch (error) {
      console.warn('Failed to load round timer state from sessionStorage:', error);
    }
    return null;
  }

  clearRoundTimerState(): void {
    try {
      sessionStorage.removeItem(this.TIMER_STORAGE_KEY);
    } catch (error) {
      console.warn('Failed to clear round timer state from sessionStorage:', error);
    }
  }
}
