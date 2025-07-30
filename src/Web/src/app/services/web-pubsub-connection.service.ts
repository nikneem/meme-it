import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface WebPubSubConnectionRequest {
  gameCode: string;
  playerId: string;
}

export interface WebPubSubConnectionResponse {
  connectionUrl: string;
  hubName: string;
  groupName: string;
  isSuccess: boolean;
  errorMessage?: string;
}

@Injectable({
  providedIn: 'root'
})
export class WebPubSubConnectionService {
  private readonly baseUrl = 'https://localhost:7294/games'; // Adjust based on your API

  constructor(private http: HttpClient) {}

  getConnectionUrl(gameCode: string, playerId: string): Observable<WebPubSubConnectionResponse> {
    const request: WebPubSubConnectionRequest = {
      gameCode,
      playerId
    };

    return this.http.post<WebPubSubConnectionResponse>(`${this.baseUrl}/connection`, request);
  }
}
