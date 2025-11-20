import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GameService } from '../../../services/game.service';
import { interval, Subscription } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { NotificationService } from '@services/notification.service';
import { GameResponse, Player } from '@models/game.model';

@Component({
  selector: 'memeit-game-lobby',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './game-lobby.html',
  styleUrl: './game-lobby.scss',
})
export class GameLobbyPage implements OnInit, OnDestroy {
  gameCode: string = '';
  game: GameResponse | null = null;
  isLoading = true;
  errorMessage = '';
  private pollSubscription?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private gameService: GameService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.gameCode = this.route.snapshot.paramMap.get('code') || '';

    if (!this.gameCode) {
      this.router.navigate(['/']);
      return;
    }

    this.loadGame();
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.stopPolling();
  }

  loadGame(): void {
    this.gameService.getGame(this.gameCode).subscribe({
      next: (game) => {
        this.game = game;
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to load game. The game may not exist or you may not have access.';
        this.notificationService.error(
          'Game Not Found',
          'Failed to load game. The game may not exist or you may not have access.',
          undefined,
          8000
        );
        console.error('Game load error:', error);
      }
    });
  }

  startPolling(): void {
    // Poll every 3 seconds to get updated player list
    this.pollSubscription = interval(3000)
      .pipe(switchMap(() => this.gameService.getGame(this.gameCode)))
      .subscribe({
        next: (game) => {
          this.game = game;
        },
        error: (error) => {
          console.error('Polling error:', error);
        }
      });
  }

  stopPolling(): void {
    if (this.pollSubscription) {
      this.pollSubscription.unsubscribe();
    }
  }

  copyGameCode(): void {
    navigator.clipboard.writeText(this.gameCode);
  }

  get currentPlayer(): Player | undefined {
    return this.game?.players.find(p => p.isHost);
  }

  get isHost(): boolean {
    return this.currentPlayer?.isHost || false;
  }

  startGame(): void {
    // TODO: Implement start game logic
    console.log('Starting game...');
  }

  leaveGame(): void {
    this.router.navigate(['/']);
  }
}
