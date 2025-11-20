import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
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

    this.loadGame();
  }

  ngOnDestroy(): void {
    // No cleanup needed
  }

  loadGame(): void {
    console.log('Loading game with code:', this.gameCode);
    this.gameService.getGame(this.gameCode).subscribe({
      next: (game) => {
        console.log('Game loaded successfully:', game);
        this.game = game;
        this.isLoading = false;
        console.log('Component state - isLoading:', this.isLoading, 'game:', this.game);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Game load error:', error);
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
    this.router.navigate(['/']);
  }
}
