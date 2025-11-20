import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateGameRequest, JoinGameRequest, GameResponse } from '../models/game.model';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly apiUrl = 'http://localhost:5000/games';

  constructor(private http: HttpClient) { }

  createGame(request: CreateGameRequest): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}`, request);
  }

  joinGame(request: JoinGameRequest): Observable<GameResponse> {
    return this.http.post<GameResponse>(`${this.apiUrl}/join`, request);
  }

  getGame(gameCode: string): Observable<GameResponse> {
    return this.http.get<GameResponse>(`${this.apiUrl}/${gameCode}`);
  }
}
