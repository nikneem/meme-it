import { Component, OnInit, OnDestroy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CanComponentDeactivate } from '../../../guards/can-deactivate-game.guard';
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
import { GameStore } from '../../../store/game.store';

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
export class GameLobbyPage implements OnInit, OnDestroy, CanComponentDeactivate {
  // Inject the game store
  readonly gameStore = inject(GameStore);

  // Store signals for template binding
  readonly gameCode = this.gameStore.gameCode;
  readonly players = this.gameStore.players;
  readonly isAdmin = this.gameStore.isAdmin;
  readonly allPlayersReady = this.gameStore.allPlayersReady;
  readonly isLoading = this.gameStore.isLoading;

  errorMessage = '';
  private realtimeSubscriptions: Subscription[] = [];
  private hasJoinedRealtimeGroup = false;
  private autoReadyTriggered = false;
  private gameCodeValue: string = '';

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
    this.gameCodeValue = this.route.snapshot.paramMap.get('code') || '';

    if (!this.gameCodeValue) {
      this.router.navigate(['/']);
      return;
    }

    // Initialize the game store - this will restore from session storage if available
    this.gameStore.initializeGame(this.gameCodeValue);

    // Connect to SignalR after a short delay to allow store initialization
    setTimeout(() => {
      if (!this.hasJoinedRealtimeGroup) {
        this.connectToRealtime().then(() => {
          this.checkAndApplyAutoReady();
        }).catch(err => {
          console.error('Failed to connect to realtime:', err);
        });
      }
    }, 100);
  }

  ngOnDestroy(): void {
    // Cleanup subscriptions
    this.realtimeSubscriptions.forEach(sub => sub.unsubscribe());

    // Only leave the group, don't disconnect (game-play page will reuse connection)
    if (this.hasJoinedRealtimeGroup && this.gameCodeValue) {
      this.realtimeService.leaveGameGroup(this.gameCodeValue).catch(err =>
        console.error('Error leaving game group:', err)
      );
    }
  }

  private async connectToRealtime(): Promise<void> {
    try {
      // Connect to SignalR hub
      await this.realtimeService.connect();

      // Join the game group
      await this.realtimeService.joinGameGroup(this.gameCodeValue);
      this.hasJoinedRealtimeGroup = true;

      // Subscribe to real-time events
      this.setupRealtimeEventHandlers();

      console.log('Connected to realtime service for game:', this.gameCodeValue);
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
      if (this.gameCodeValue) {
        await this.realtimeService.leaveGameGroup(this.gameCodeValue);
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
      this.gameStore.handlePlayerJoined(event);
      this.notificationService.success(
        'Player Joined',
        `${event.displayName} joined the game`,
        undefined,
        3000
      );
    });
    this.realtimeSubscriptions.push(playerJoinedSub);

    // Handle player state changed events
    const playerStateChangedSub = this.realtimeService.playerStateChanged$.subscribe(event => {
      console.log('Player state changed:', event);
      this.gameStore.handlePlayerStateChanged(event);

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
          await this.realtimeService.leaveGameGroup(this.gameCodeValue);
          await this.realtimeService.disconnect();
        } catch (err) {
          console.error('Error while leaving realtime group after kick:', err);
        }
        this.gameStore.clearGame(this.gameCodeValue);
        this.router.navigate(['/']);
        return;
      }

      // For other players update store
      this.gameStore.handlePlayerRemoved(event);
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
      this.gameStore.handleGameStarted(event);
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
    if (this.gameStore.phase() !== 'lobby') {
      return;
    }

    // Find current player and check if already ready
    const currentPlayer = this.players().find(p => p.playerId === this.currentUserId);
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
    const joinUrl = `${window.location.origin}/games/join/${this.gameCodeValue}`;
    navigator.clipboard.writeText(joinUrl);
    this.notificationService.success('Copied!', 'Join link copied to clipboard', undefined, 2000);
  }

  get playerCount(): number {
    return this.players().length;
  }

  get readyPlayerCount(): number {
    return this.players().filter(p => p.isReady).length;
  }

  get currentUserId(): string | null {
    return this.authService.getCurrentUserId();
  }

  isCurrentUser(playerId: string): boolean {
    return this.currentUserId === playerId;
  }

  isPlayerAdmin(playerId: string): boolean {
    return this.players()[0]?.playerId === playerId;
  }

  setPlayerReady(): void {
    this.gameService.setPlayerReady(this.gameCodeValue, true).subscribe({
      next: () => {
        console.log('Player set to ready');
        // State will be updated via SignalR event
      },
      error: (error) => {
        console.error('Failed to set player ready:', error);
        this.notificationService.error('Error', 'Failed to set player ready');
      }
    });
  }

  toggleReady(player: any): void {
    if (!this.isCurrentUser(player.playerId)) return;

    this.gameService.setPlayerReady(this.gameCodeValue, !player.isReady).subscribe({
      next: () => {
        console.log('Ready state updated');
        // State will be updated via SignalR event
      },
      error: (error) => {
        console.error('Failed to update ready state:', error);
        this.notificationService.error('Error', 'Failed to update ready state');
      }
    });
  }

  removePlayer(playerId: string): void {
    if (!this.isAdmin()) return;

    this.gameService.removePlayer(this.gameCodeValue, playerId).subscribe({
      next: () => {
        console.log('Player removed');
        // State will be updated via SignalR event
        this.notificationService.success('Success', 'Player removed from game');
      },
      error: (error) => {
        console.error('Failed to remove player:', error);
        this.notificationService.error('Error', 'Failed to remove player');
      }
    });
  }

  startGame(): void {
    if (!this.isAdmin()) {
      this.notificationService.error('Error', 'Only the game admin can start the game');
      return;
    }

    this.gameService.startGame(this.gameCodeValue).subscribe({
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

    // Clear the game state from store and session storage
    this.gameStore.clearGame(this.gameCodeValue);
    this.router.navigate(['/']);
  }

  canDeactivate(): boolean {
    // Always show confirmation when leaving lobby
    // Return false to trigger the guard dialog
    return false;
  }
}
