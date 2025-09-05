import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Game, CreateGameRequest, JoinGameRequest, Player, GameStatus, GameSettings } from '../store/game/game.models';

export interface CreateGameResponse {
  game: Game;
  player: Player;
}

export interface JoinGameResponse {
  game: Game;
  player: Player;
}

export interface GameValidationResponse {
  game: Game;
  player: Player;
  isValid: boolean;
}

// Backend response interfaces (matching the C# backend)
interface BackendPlayerResponse {
  id: string;
  name: string;
  isReady: boolean;
}

interface BackendCreateGameResponse {
  gameCode: string;
  status: string;
  players: BackendPlayerResponse[];
  playerId: string;
  isPasswordProtected: boolean;
  settings: {
    maxPlayers: number;
    numberOfRounds: number;
    category: string;
  };
}

interface BackendGameDetailsResponse {
  gameCode: string;
  status: string;
  players: BackendPlayerResponse[];
  playerId: string;
  isPasswordProtected: boolean;
  settings: {
    maxPlayers: number;
    numberOfRounds: number;
    category: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly baseUrl = 'https://localhost:7294/games'; // Adjust based on your API

  constructor(private http: HttpClient) {}

  createGame(request: CreateGameRequest): Observable<CreateGameResponse> {
    return this.http.post<BackendCreateGameResponse>(`${this.baseUrl}`, request).pipe(
      map(response => this.mapBackendResponseToFrontend(response))
    );
  }

  joinGame(request: JoinGameRequest): Observable<JoinGameResponse> {
    return this.http.post<BackendGameDetailsResponse>(`${this.baseUrl}/join`, request).pipe(
      map(response => this.mapBackendResponseToFrontend(response))
    );
  }

  leaveGame(gameCode: string, playerId: string): Observable<void> {
    const body = { gameCode, playerId };
    return this.http.post<void>(`${this.baseUrl}/leave`, body);
  }

  setPlayerReadyStatus(gameCode: string, playerId: string, isReady: boolean): Observable<CreateGameResponse> {
    const request = { playerId, gameCode, isReady };
    return this.http.post<BackendGameDetailsResponse>(`${this.baseUrl}/ready`, request).pipe(
      map(response => this.mapBackendResponseToFrontend(response))
    );
  }

  kickPlayer(gameCode: string, hostPlayerId: string, targetPlayerId: string): Observable<Game> {
    const request = { gameCode, hostPlayerId, targetPlayerId };
    return this.http.post<BackendGameDetailsResponse>(`${this.baseUrl}/kick`, request).pipe(
      map(response => this.mapBackendGameResponseToGame(response))
    );
  }

  startGame(gameCode: string, playerId: string): Observable<Game> {
    const request = { playerId, gameCode };
    return this.http.post<BackendGameDetailsResponse>(`${this.baseUrl}/start`, request).pipe(
      map(response => this.mapBackendGameResponseToGame(response))
    );
  }

  getGame(gameId: string, playerId?: string): Observable<Game> {
    const url = playerId ? `${this.baseUrl}/${gameId}?playerId=${playerId}` : `${this.baseUrl}/${gameId}`;
    return this.http.get<BackendGameDetailsResponse>(url).pipe(
      map(response => this.mapBackendGameResponseToGame(response))
    );
  }

  getRandomMemeForPlayer(gameCode: string, playerId: string): Observable<any> {
    const request = { gameCode, playerId };
    return this.http.post<any>(`${this.baseUrl}/random-memes`, request);
  }

  validateGameAndPlayer(gameCode: string, playerId: string, playerName: string): Observable<GameValidationResponse> {
    console.log('GameService.validateGameAndPlayer called with:', { gameCode, playerId, playerName });
    return this.getGame(gameCode, playerId).pipe(
      map(game => {
        console.log('GameService: Backend returned game with players:', game.players.map(p => ({ id: p.id, name: p.name })));
        
        // First try exact match (ID and name)
        let player = game.players.find(p => p.id === playerId && p.name === playerName);
        
        // If exact match fails, try ID-only match (more forgiving for refresh scenarios)
        if (!player) {
          console.log('GameService: Exact match failed, trying ID-only match');
          player = game.players.find(p => p.id === playerId);
          
          if (player) {
            console.log('GameService: Found player by ID only, name mismatch detected:', {
              storedName: playerName,
              currentName: player.name
            });
          }
        }
        
        console.log('GameService: Player validation result:', { 
          foundPlayer: !!player, 
          searchedForId: playerId, 
          searchedForName: playerName,
          foundPlayerId: player?.id,
          foundPlayerName: player?.name
        });
        
        return {
          game,
          player: player || { id: playerId, name: playerName, isHost: false, isReady: false },
          isValid: !!player
        };
      })
    );
  }

  private mapBackendResponseToFrontend(response: BackendCreateGameResponse | BackendGameDetailsResponse): CreateGameResponse {
    const game = this.mapBackendGameResponseToGame(response);
    const currentPlayer = game.players.find(p => p.id === response.playerId);
    
    return {
      game,
      player: currentPlayer || {
        id: response.playerId,
        name: response.players[0]?.name || 'Unknown',
        isHost: true, // First player is typically the host
        isReady: false
      }
    };
  }

  private mapBackendGameResponseToGame(response: BackendGameDetailsResponse): Game {
    // Provide default settings if backend doesn't send them
    const defaultSettings = {
      maxPlayers: 8,
      numberOfRounds: 5,
      category: 'general'
    };
    
    const settings = response.settings || defaultSettings;
    
    return {
      id: response.gameCode,
      gameCode: response.gameCode,
      name: `Game ${response.gameCode}`, // Backend doesn't provide name, so we create one
      status: this.mapBackendStatusToGameStatus(response.status),
      hasPassword: response.isPasswordProtected,
      maxPlayers: settings.maxPlayers,
      currentPlayers: response.players?.length || 0,
      createdAt: new Date(), // Backend doesn't provide this, use current time
      host: {
        id: response.players?.[0]?.id || response.playerId || '',
        name: response.players?.[0]?.name || 'Host',
        isHost: true,
        isReady: response.players?.[0]?.isReady || false
      },
      players: (response.players || []).map((player, index) => ({
        id: player.id,
        name: player.name,
        isHost: index === 0, // First player is the host
        isReady: player.isReady
      })),
      settings: {
        maxPlayers: settings.maxPlayers,
        timePerRound: 30, // Default value, backend doesn't provide this
        totalRounds: settings.numberOfRounds,
        allowsSpectators: false // Default value, backend doesn't provide this
      }
    };
  }

  private mapBackendStatusToGameStatus(backendStatus: string): GameStatus {
    switch (backendStatus.toLowerCase()) {
      case 'waiting':
        return GameStatus.Waiting;
      case 'active':
      case 'in-progress':
        return GameStatus.InProgress;
      case 'finished':
        return GameStatus.Finished;
      case 'cancelled':
        return GameStatus.Cancelled;
      default:
        return GameStatus.Waiting;
    }
  }
}
