import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SavePlayerMemeRequest {
  gameCode: string;
  memeTemplateId: string;
  texts: string[];
  roundNumber: number;
}

export interface SavePlayerMemeResponse {
  id: string;
  gameCode: string;
  playerId: string;
  memeTemplateId: string;
  texts: string[];
  roundNumber: number;
  createdAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class GameMemeService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://localhost:7294/games'; // This will be proxied in development

  /**
   * Saves a player's completed meme for the current round
   */
  savePlayerMeme(request: SavePlayerMemeRequest): Observable<SavePlayerMemeResponse> {
    // TODO: Implement the actual API endpoint for saving player memes
    // For now, we'll simulate the save operation
    return new Observable<SavePlayerMemeResponse>(observer => {
      setTimeout(() => {
        const response: SavePlayerMemeResponse = {
          id: this.generateId(),
          gameCode: request.gameCode,
          playerId: 'current-player-id', // TODO: Get from current player state
          memeTemplateId: request.memeTemplateId,
          texts: request.texts,
          roundNumber: request.roundNumber,
          createdAt: new Date()
        };
        observer.next(response);
        observer.complete();
      }, 1000); // Simulate API delay
    });
  }

  private generateId(): string {
    return 'meme-' + Math.random().toString(36).substr(2, 9);
  }
}
