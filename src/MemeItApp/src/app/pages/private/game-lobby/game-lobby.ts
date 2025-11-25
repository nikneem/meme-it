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
import { RealtimeService } from '@services/realtime.service';
import { SettingsService } from '@services/settings.service';
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
  private realtimeSubscriptions: Subscription[] = [];
  private hasJoinedRealtimeGroup = false;
  private autoReadyTriggered = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private gameService: GameService,
    private notificationService: NotificationService,
    private authService: AuthService,
    private realtimeService: RealtimeService,
    private settingsService: SettingsService,
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

          // Connect to SignalR and join game group after successfully loading game (only once)
          if (!this.hasJoinedRealtimeGroup) {
            this.connectToRealtime().then(() => {
              // After connecting to realtime, check auto-ready setting
              this.checkAndApplyAutoReady();
            }).catch(err => {
              console.error('Failed to connect to realtime:', err);
            });
          }
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
    // Cleanup subscriptions
    if (this.gameStateSubscription) {
      this.gameStateSubscription.unsubscribe();
    }

    this.realtimeSubscriptions.forEach(sub => sub.unsubscribe());

    // Only leave the group, don't disconnect (game-play page will reuse connection)
    if (this.hasJoinedRealtimeGroup && this.gameCode) {
      this.realtimeService.leaveGameGroup(this.gameCode).catch(err =>
        console.error('Error leaving game group:', err)
      );
    }
  }

  private async connectToRealtime(): Promise<void> {
    try {
      // Connect to SignalR hub
      await this.realtimeService.connect();

      // Join the game group
      await this.realtimeService.joinGameGroup(this.gameCode);
      this.hasJoinedRealtimeGroup = true;

      // Subscribe to real-time events
      this.setupRealtimeEventHandlers();

      console.log('Connected to realtime service for game:', this.gameCode);
    } catch (error) {
      console.error('Failed to connect to realtime service:', error);
      this.hasJoinedRealtimeGroup = false;
      this.notificationService.error(
        'Connection Error',
        'Failed to connect to real-time updates. Some features may not work.',
        undefined,
        5000
      );
    }
  }

  private async disconnectFromRealtime(): Promise<void> {
    try {
      if (this.gameCode) {
        await this.realtimeService.leaveGameGroup(this.gameCode);
      }
      await this.realtimeService.disconnect();
    } catch (error) {
      console.error('Error disconnecting from realtime service:', error);
    }
  }

  private setupRealtimeEventHandlers(): void {
    // Handle player joined events
    const playerJoinedSub = this.realtimeService.playerJoined$.subscribe(event => {
      console.log('Player joined:', event);
      this.notificationService.success(
        'Player Joined',
        `${event.displayName} joined the game`,
        undefined,
        3000
      );
      // Refresh game state
      this.loadGame();
    });
    this.realtimeSubscriptions.push(playerJoinedSub);

    // Handle player state changed events
    const playerStateChangedSub = this.realtimeService.playerStateChanged$.subscribe(event => {
      console.log('Player state changed:', event);
      if (this.game) {
        const player = this.game.players.find(p => p.playerId === event.playerId);
        if (player) {
          player.isReady = event.isReady;
          this.cdr.detectChanges();
        }
      }

      const status = event.isReady ? 'ready' : 'not ready';
      this.notificationService.success(
        'Player Status',
        `${event.displayName} is ${status}`,
        undefined,
        2000
      );
    });
    this.realtimeSubscriptions.push(playerStateChangedSub);

    // Handle player removed events
    const playerRemovedSub = this.realtimeService.playerRemoved$.subscribe(async event => {
      console.log('Player removed:', event);

      // If the current user was removed (kicked), notify and redirect
      if (event.playerId === this.currentUserId) {
        this.notificationService.error(
          'Removed From Game',
          'You were kicked from the game.',
          undefined,
          5000
        );
        try {
          await this.realtimeService.leaveGameGroup(this.gameCode);
          await this.realtimeService.disconnect();
        } catch (err) {
          console.error('Error while leaving realtime group after kick:', err);
        }
        this.gameService.clearGameState(this.gameCode);
        this.router.navigate(['/']);
        return; // Do not process further as user is leaving
      }

      // For other players simply update local state
      if (this.game) {
        this.game.players = this.game.players.filter(p => p.playerId !== event.playerId);
        this.cdr.detectChanges();
      }

      this.notificationService.success(
        'Player Left',
        `${event.displayName} left the game`,
        undefined,
        3000
      );
    });
    this.realtimeSubscriptions.push(playerRemovedSub);

    // Handle game started events
    const gameStartedSub = this.realtimeService.gameStarted$.subscribe(event => {
      console.log('Game started:', event);
      this.notificationService.success(
        'Game Started!',
        'The game has begun. Good luck!',
        undefined,
        3000
      );
      // Navigate to the play page
      this.router.navigate([`/app/games/${event.gameCode}/play`]);
    });
    this.realtimeSubscriptions.push(gameStartedSub);
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

  private checkAndApplyAutoReady(): void {
    // Only trigger once per lobby session
    if (this.autoReadyTriggered) {
      return;
    }

    // Check if auto-ready is enabled in settings
    const settings = this.settingsService.getCurrentSettings();
    if (!settings.user.autoReady) {
      return;
    }

    // Only auto-ready if game is in lobby state
    if (!this.game || this.game.state !== 'Lobby') {
      return;
    }

    // Find current player and check if already ready
    const currentPlayer = this.game.players.find(p => p.playerId === this.currentUserId);
    if (!currentPlayer || currentPlayer.isReady) {
      return;
    }

    // Mark flag to prevent multiple triggers
    this.autoReadyTriggered = true;

    // Automatically set player ready
    console.log('Auto-ready enabled, setting player ready');
    this.setPlayerReady();
  }

  copyGameCode(): void {
    const joinUrl = `${window.location.origin}/games/join/${this.gameCode}`;
    navigator.clipboard.writeText(joinUrl);
    this.notificationService.success('Copied!', 'Join link copied to clipboard', undefined, 2000);
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
    if (!this.isAdmin) {
      this.notificationService.error('Error', 'Only the game admin can start the game');
      return;
    }

    this.gameService.startGame(this.gameCode).subscribe({
      next: (result) => {
        console.log('Game started successfully:', result);
        // The navigation will happen via the GameStarted event from SignalR
      },
      error: (error) => {
        console.error('Failed to start game:', error);
        this.notificationService.error('Error', 'Failed to start game. Please try again.');
      }
    });
  }

  leaveGame(): void {
    // Disconnect from realtime before leaving
    this.disconnectFromRealtime();

    // Clear the cached game state when leaving
    this.gameService.clearGameState(this.gameCode);
    this.router.navigate(['/']);
  }
}
