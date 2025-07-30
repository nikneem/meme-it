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
import { BreakoutGameComponent } from '../../shared/components/breakout-game/breakout-game.component';

@Component({
  selector: 'app-game-lobby',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ButtonModule,
    CardModule,
    TagModule,
    DividerModule,
    BreakoutGameComponent
  ],
  templateUrl: './game-lobby.component.html',
  styleUrl: './game-lobby.component.scss'
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
      case 'waiting':
      case 'Waiting':
        return 'info';
      case 'in-progress':
      case 'InProgress':
      case 'active':
      case 'Active':
        return 'success';
      case 'finished':
      case 'Finished':
        return 'success';
      case 'cancelled':
      case 'Cancelled':
        return 'danger';
      default:
        return 'info';
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
