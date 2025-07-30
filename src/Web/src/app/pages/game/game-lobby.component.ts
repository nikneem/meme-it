import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';

import { Game, Player } from '../../store/game/game.models';
import { 
  selectCurrentGame, 
  selectCurrentPlayer, 
  selectIsHost, 
  selectCanStartGame,
  selectPlayerCount,
  selectGameCode 
} from '../../store/game/game.selectors';
import { leaveGame } from '../../store/game/game.actions';

@Component({
  selector: 'app-game-lobby',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ButtonModule,
    CardModule,
    TagModule,
    DividerModule
  ],
  template: `
    <div class="min-h-screen bg-gradient-to-br from-purple-600 via-blue-600 to-cyan-500 p-4">
      <div class="max-w-4xl mx-auto">
        
        <!-- Header -->
        <div class="text-center mb-8">
          <h1 class="text-4xl font-bold text-white mb-4">Game Lobby</h1>
          <div class="bg-white/20 backdrop-blur-sm rounded-lg p-4 inline-block" *ngIf="gameCode$ | async as gameCode">
            <p class="text-white text-lg mb-2">Game Code</p>
            <p class="text-3xl font-mono font-bold text-yellow-300">{{ gameCode }}</p>
          </div>
        </div>

        <!-- Game Info -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8" *ngIf="currentGame$ | async as game">
          
          <!-- Game Details -->
          <p-card header="Game Details" styleClass="bg-white/10 backdrop-blur-sm border-0">
            <div class="space-y-3 text-white">
              <div class="flex justify-between">
                <span>Game Name:</span>
                <span class="font-semibold">{{ game.name }}</span>
              </div>
              <div class="flex justify-between">
                <span>Status:</span>
                <p-tag [value]="game.status" [severity]="getStatusSeverity(game.status)"></p-tag>
              </div>
              <div class="flex justify-between" *ngIf="playerCount$ | async as count">
                <span>Players:</span>
                <span class="font-semibold">{{ count.current }} / {{ count.max }}</span>
              </div>
              <div class="flex justify-between">
                <span>Rounds:</span>
                <span class="font-semibold">{{ game.settings.totalRounds }}</span>
              </div>
              <div class="flex justify-between">
                <span>Time per Round:</span>
                <span class="font-semibold">{{ game.settings.timePerRound }}s</span>
              </div>
              <div class="flex justify-between">
                <span>Password Protected:</span>
                <span class="font-semibold">{{ game.hasPassword ? 'Yes' : 'No' }}</span>
              </div>
            </div>
          </p-card>

          <!-- Host Actions -->
          <p-card header="Game Actions" styleClass="bg-white/10 backdrop-blur-sm border-0" *ngIf="isHost$ | async">
            <div class="space-y-4">
              <p class="text-white">As the host, you can start the game when all players are ready.</p>
              <p-button 
                label="Start Game" 
                icon="pi pi-play"
                styleClass="w-full"
                [disabled]="!(canStartGame$ | async)"
                (onClick)="startGame()">
              </p-button>
              <p class="text-sm text-white/70" *ngIf="!(canStartGame$ | async)">
                All players must be ready and at least 2 players are required to start.
              </p>
            </div>
          </p-card>

          <!-- Player Info (for non-hosts) -->
          <p-card header="Ready Status" styleClass="bg-white/10 backdrop-blur-sm border-0" *ngIf="!(isHost$ | async)">
            <div class="space-y-4">
              <p class="text-white">Mark yourself as ready when you're prepared to play.</p>
              <p-button 
                [label]="(currentPlayer$ | async)?.isReady ? 'Ready!' : 'Mark as Ready'"
                [icon]="(currentPlayer$ | async)?.isReady ? 'pi pi-check' : 'pi pi-clock'"
                styleClass="w-full"
                [severity]="(currentPlayer$ | async)?.isReady ? 'success' : 'secondary'"
                (onClick)="toggleReady()">
              </p-button>
            </div>
          </p-card>
        </div>

        <!-- Players List -->
        <p-card header="Players" styleClass="bg-white/10 backdrop-blur-sm border-0 mb-8" *ngIf="currentGame$ | async as game">
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div 
              *ngFor="let player of game.players" 
              class="bg-white/20 rounded-lg p-4 flex items-center justify-between">
              <div class="flex items-center space-x-3">
                <div class="w-10 h-10 bg-gradient-to-r from-pink-500 to-violet-500 rounded-full flex items-center justify-center">
                  <span class="text-white font-bold">{{ player.name.charAt(0).toUpperCase() }}</span>
                </div>
                <div>
                  <p class="text-white font-semibold">{{ player.name }}</p>
                  <p class="text-white/70 text-sm" *ngIf="player.isHost">Host</p>
                </div>
              </div>
              <div class="flex items-center space-x-2">
                <p-tag 
                  [value]="player.isReady ? 'Ready' : 'Not Ready'"
                  [severity]="player.isReady ? 'success' : 'secondary'">
                </p-tag>
              </div>
            </div>
          </div>
        </p-card>

        <!-- Actions -->
        <div class="text-center">
          <p-button 
            label="Leave Game" 
            icon="pi pi-sign-out"
            severity="danger"
            styleClass="w-full sm:w-auto"
            (onClick)="onLeaveGame()">
          </p-button>
        </div>
      </div>
    </div>
  `
})
export class GameLobbyComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  currentGame$: Observable<Game | null>;
  currentPlayer$: Observable<Player | null>;
  isHost$: Observable<boolean>;
  canStartGame$: Observable<boolean>;
  playerCount$: Observable<{ current: number; max: number }>;
  gameCode$: Observable<string | undefined>;

  constructor(private store: Store) {
    this.currentGame$ = this.store.select(selectCurrentGame);
    this.currentPlayer$ = this.store.select(selectCurrentPlayer);
    this.isHost$ = this.store.select(selectIsHost);
    this.canStartGame$ = this.store.select(selectCanStartGame);
    this.playerCount$ = this.store.select(selectPlayerCount);
    this.gameCode$ = this.store.select(selectGameCode);
  }

  ngOnInit() {
    // Set up any real-time connections here (SignalR, WebSocket, etc.)
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getStatusSeverity(status: string): 'success' | 'info' | 'warning' | 'danger' {
    switch (status) {
      case 'waiting': return 'info';
      case 'in-progress': return 'success';
      case 'finished': return 'success';
      case 'cancelled': return 'danger';
      default: return 'info';
    }
  }

  startGame() {
    // Dispatch action to start game
    console.log('Starting game...');
  }

  toggleReady() {
    // Dispatch action to toggle ready status
    console.log('Toggling ready status...');
  }

  onLeaveGame() {
    this.store.dispatch(leaveGame());
  }
}
