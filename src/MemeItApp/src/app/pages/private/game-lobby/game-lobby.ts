import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { Subscription } from 'rxjs';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { AuthService } from '@services/auth.service';
import { GameResponse, Player } from '@models/game.model';

@Component({
  selector: 'memeit-game-lobby',
  imports: [
    CommonModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSlideToggleModule
  ],
  templateUrl: './game-lobby.html',
  styleUrl: './game-lobby.scss',
})
export class GameLobbyPage implements OnInit, OnDestroy {
  gameCode: string = '';
  game: GameResponse | null = null;
  isLoading = true;
  errorMessage = '';
  private gameStateSubscription?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private gameService: GameService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.gameCode = this.route.snapshot.paramMap.get('code') || '';

    if (!this.gameCode) {
      this.router.navigate(['/']);
      return;
    }

    // Subscribe to the game state observable
    this.gameStateSubscription = this.gameService.getGameState$(this.gameCode).subscribe({
      next: (game) => {
        if (game) {
          console.log('Game state updated:', game);
          this.game = game;
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      },
      error: (error) => {
        console.error('Game state error:', error);
        this.isLoading = false;
        this.errorMessage = 'Failed to load game. The game may not exist or you may not have access.';
        this.notificationService.error(
          'Game Not Found',
          'Failed to load game. The game may not exist or you may not have access.',
          undefined,
          8000
        );
        this.cdr.detectChanges();
      }
    });
  }

  ngOnDestroy(): void {
    // Cleanup subscription
    if (this.gameStateSubscription) {
      this.gameStateSubscription.unsubscribe();
    }
  }

  loadGame(): void {
    console.log('Refreshing game with code:', this.gameCode);
    this.gameService.refreshGame(this.gameCode).subscribe({
      next: (game) => {
        console.log('Game refreshed successfully:', game);
        // State is automatically updated via the subscription
      },
      error: (error) => {
        console.error('Game refresh error:', error);
        this.isLoading = false;
        this.errorMessage = 'Failed to load game. The game may not exist or you may not have access.';
        this.notificationService.error(
          'Game Not Found',
          'Failed to load game. The game may not exist or you may not have access.',
          undefined,
          8000
        );
        this.cdr.detectChanges();
      }
    });
  }

  copyGameCode(): void {
    navigator.clipboard.writeText(this.gameCode);
    this.notificationService.success('Copied!', 'Game code copied to clipboard', undefined, 2000);
  }

  get isAdmin(): boolean {
    return this.game?.isAdmin || false;
  }

  get playerCount(): number {
    return this.game?.players.length || 0;
  }

  get readyPlayerCount(): number {
    return this.game?.players.filter(p => p.isReady).length || 0;
  }

  get allPlayersReady(): boolean {
    if (!this.game || this.game.players.length < 2) return false;
    return this.game.players.every(p => p.isReady);
  }

  get currentUserId(): string | null {
    return this.authService.getCurrentUserId();
  }

  isCurrentUser(playerId: string): boolean {
    return this.currentUserId === playerId;
  }

  isPlayerAdmin(playerId: string): boolean {
    return this.game?.players[0]?.playerId === playerId;
  }

  setPlayerReady(): void {
    this.gameService.setPlayerReady(this.gameCode, true).subscribe({
      next: () => {
        console.log('Player set to ready');
        this.loadGame();
      },
      error: (error) => {
        console.error('Failed to set player ready:', error);
        this.notificationService.error('Error', 'Failed to set player ready');
      }
    });
  }

  toggleReady(player: any): void {
    if (!this.isCurrentUser(player.playerId)) return;

    this.gameService.setPlayerReady(this.gameCode, !player.isReady).subscribe({
      next: () => {
        console.log('Ready state updated');
        player.isReady = !player.isReady;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Failed to update ready state:', error);
        this.notificationService.error('Error', 'Failed to update ready state');
      }
    });
  }

  removePlayer(playerId: string): void {
    if (!this.isAdmin) return;

    this.gameService.removePlayer(this.gameCode, playerId).subscribe({
      next: () => {
        console.log('Player removed');
        if (this.game) {
          this.game.players = this.game.players.filter(p => p.playerId !== playerId);
          this.cdr.detectChanges();
        }
        this.notificationService.success('Success', 'Player removed from game');
      },
      error: (error) => {
        console.error('Failed to remove player:', error);
        this.notificationService.error('Error', 'Failed to remove player');
      }
    });
  }

  startGame(): void {
    // TODO: Implement start game logic
    console.log('Starting game...');
  }

  leaveGame(): void {
    // Clear the cached game state when leaving
    this.gameService.clearGameState(this.gameCode);
    this.router.navigate(['/']);
  }
}
