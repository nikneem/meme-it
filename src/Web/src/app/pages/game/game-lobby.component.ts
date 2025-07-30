import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject } from 'rxjs';
import { takeUntil, take } from 'rxjs/operators';

import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

import { Game, Player } from '../../store/game/game.models';
import { WebPubSubService, WebSocketConnectionStatus } from '../../services/web-pubsub.service';
import { 
  selectCurrentGame, 
  selectCurrentPlayer, 
  selectIsHost, 
  selectCanStartGame,
  selectPlayerCount,
  selectGameCode 
} from '../../store/game/game.selectors';
import { leaveGame, setPlayerReadyStatus, startGame, kickPlayer, ensureWebPubSubConnection } from '../../store/game/game.actions';
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
    ConfirmDialogModule,
    BreakoutGameComponent
  ],
  providers: [ConfirmationService],
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
  
  // WebPubSub connection status
  webPubSubConnectionStatus$: Observable<WebSocketConnectionStatus>;

  constructor(
    private store: Store, 
    private confirmationService: ConfirmationService,
    private webPubSubService: WebPubSubService
  ) {
    this.currentGame$ = this.store.select(selectCurrentGame);
    this.currentPlayer$ = this.store.select(selectCurrentPlayer);
    this.isHost$ = this.store.select(selectIsHost);
    this.canStartGame$ = this.store.select(selectCanStartGame);
    this.playerCount$ = this.store.select(selectPlayerCount);
    this.gameCode$ = this.store.select(selectGameCode);
    this.webPubSubConnectionStatus$ = this.webPubSubService.connectionStatus$;
  }

  ngOnInit() {
    // Ensure WebPubSub connection when entering the lobby
    this.store.dispatch(ensureWebPubSubConnection());
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
    // Dispatch action to start game - effect will get game code and player ID from state
    this.store.dispatch(startGame());
  }

  toggleReady() {
    this.currentPlayer$.pipe(
      take(1) // Only take the current value once
    ).subscribe((player: Player | null) => {
      if (player) {
        this.store.dispatch(setPlayerReadyStatus({ isReady: !player.isReady }));
      }
    });
  }

  kickPlayer(targetPlayerId: string) {
    this.confirmationService.confirm({
      message: 'Are you sure you want to kick this player from the game?',
      header: 'Kick Player',
      icon: 'pi pi-exclamation-triangle',
      acceptIcon: 'pi pi-check',
      rejectIcon: 'pi pi-times',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.store.dispatch(kickPlayer({ targetPlayerId }));
      }
    });
  }

  onLeaveGame() {
    this.store.dispatch(leaveGame());
  }
}
