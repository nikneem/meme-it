import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { CreateGameRequest, JoinGameRequest, GameResponse } from '../models/game.model';
import { API_BASE_URL } from '../constants/api.constants';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly apiUrl = `${API_BASE_URL}/games`;
  private gameStateCache = new Map<string, BehaviorSubject<GameResponse | null>>();

  constructor(private http: HttpClient) { }

  createGame(request: CreateGameRequest): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}`, request);
  }

  joinGame(request: JoinGameRequest): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}/${request.gameCode}/join`, { password: request.password });
  }

  getGame(gameCode: string): Observable<GameResponse> {
    // Check if we have a cached state for this game
    if (!this.gameStateCache.has(gameCode)) {
      this.gameStateCache.set(gameCode, new BehaviorSubject<GameResponse | null>(null));
    }

    const cachedState = this.gameStateCache.get(gameCode)!;

    // If we have a cached value, return it immediately
    if (cachedState.value) {
      return of(cachedState.value);
    }

    // Otherwise, fetch from server and cache the result
    return this.http.get<GameResponse>(`${this.apiUrl}/${gameCode}`).pipe(
      tap(game => {
        cachedState.next(game);
      }),
      catchError(error => {
        // Clear cache on error
        this.gameStateCache.delete(gameCode);
        throw error;
      })
    );
  }

  /**
   * Forces a refresh of the game state from the server
   */
  refreshGame(gameCode: string): Observable<GameResponse> {
    return this.http.get<GameResponse>(`${this.apiUrl}/${gameCode}`).pipe(
      tap(game => {
        if (this.gameStateCache.has(gameCode)) {
          this.gameStateCache.get(gameCode)!.next(game);
        } else {
          this.gameStateCache.set(gameCode, new BehaviorSubject<GameResponse | null>(game));
        }
      })
    );
  }

  /**
   * Updates the local game state (useful for real-time updates via SignalR)
   */
  updateLocalGameState(gameCode: string, game: GameResponse): void {
    if (this.gameStateCache.has(gameCode)) {
      this.gameStateCache.get(gameCode)!.next(game);
    } else {
      this.gameStateCache.set(gameCode, new BehaviorSubject<GameResponse | null>(game));
    }
  }

  /**
   * Gets the current cached game state as an observable
   */
  getGameState$(gameCode: string): Observable<GameResponse | null> {
    if (!this.gameStateCache.has(gameCode)) {
      this.gameStateCache.set(gameCode, new BehaviorSubject<GameResponse | null>(null));
      // Trigger initial load
      this.refreshGame(gameCode).subscribe();
    }
    return this.gameStateCache.get(gameCode)!.asObservable();
  }

  /**
   * Clears the cached state for a game
   */
  clearGameState(gameCode: string): void {
    if (this.gameStateCache.has(gameCode)) {
      this.gameStateCache.get(gameCode)!.next(null);
      this.gameStateCache.delete(gameCode);
    }
  }

  setPlayerReady(gameCode: string, isReady: boolean): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${gameCode}/ready?isReady=${isReady}`, {});
  }

  removePlayer(gameCode: string, playerId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${gameCode}/remove-player/${playerId}`);
  }

  startGame(gameCode: string): Observable<{ gameCode: string; roundNumber: number }> {
    return this.http.post<{ gameCode: string; roundNumber: number }>(`${this.apiUrl}/${gameCode}/start`, {});
  }

  selectMemeTemplate(gameCode: string, roundNumber: number, memeTemplateId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${gameCode}/rounds/${roundNumber}/select-meme`, { memeTemplateId });
  }

  getPlayerRoundState(gameCode: string): Observable<{
    gameCode: string;
    playerId: string;
    roundNumber: number;
    roundStartedAt: string;
    selectedMemeTemplateId?: string;
  }> {
    return this.http.get<{
      gameCode: string;
      playerId: string;
      roundNumber: number;
      roundStartedAt: string;
      selectedMemeTemplateId?: string;
    }>(`${this.apiUrl}/${gameCode}/select-meme`);
  }
}
