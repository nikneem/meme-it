import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Game, CreateGameRequest, JoinGameRequest, Player } from '../store/game/game.models';

export interface CreateGameResponse {
  game: Game;
  player: Player;
}

export interface JoinGameResponse {
  game: Game;
  player: Player;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly baseUrl = 'https://localhost:7294/games'; // Adjust based on your API

  constructor(private http: HttpClient) {}

  createGame(request: CreateGameRequest): Observable<CreateGameResponse> {
    return this.http.post<CreateGameResponse>(`${this.baseUrl}`, request);
  }

  joinGame(request: JoinGameRequest): Observable<JoinGameResponse> {
    return this.http.post<JoinGameResponse>(`${this.baseUrl}/join`, request);
  }

  leaveGame(gameCode: string, playerId: string): Observable<void> {
    const body = { gameCode, playerId };
    return this.http.post<void>(`${this.baseUrl}/leave`, body);
  }

  getGame(gameId: string): Observable<Game> {
    return this.http.get<Game>(`${this.baseUrl}/${gameId}`);
  }
}
