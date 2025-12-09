import { Component, signal, OnInit, OnDestroy, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { NotificationsComponent } from './components/notifications/notifications.component';
import { SettingsComponent } from './components/settings/settings.component';
import { NewGameInvitationDialogComponent } from './components/new-game-invitation-dialog/new-game-invitation-dialog.component';
import { AudioService } from './services/audio.service';
import { RealtimeService } from './services/realtime.service';

@Component({
  selector: 'memeit-root',
  imports: [RouterOutlet, NotificationsComponent, SettingsComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit, OnDestroy {
  protected readonly title = signal('MemeItApp');
  private newGameStartedSubscription?: Subscription;
  private dialog = inject(MatDialog);
  private router = inject(Router);
  private realtimeService = inject(RealtimeService);

  constructor(private audioService: AudioService) { }

  ngOnInit(): void {
    // AudioService is injected and will automatically initialize
    // Background music will start on first user interaction if enabled

    // Subscribe to NewGameStarted events globally
    this.newGameStartedSubscription = this.realtimeService.newGameStarted$.subscribe(event => {
      console.log('NewGameStarted event received in app:', event);
      this.showNewGameInvitation(event.newGameCode, event.initiatedByPlayerName);
    });
  }

  ngOnDestroy(): void {
    this.newGameStartedSubscription?.unsubscribe();
  }

  private showNewGameInvitation(newGameCode: string, initiatedByPlayerName: string): void {
    const dialogRef = this.dialog.open(NewGameInvitationDialogComponent, {
      width: '500px',
      maxWidth: '90vw',
      disableClose: false,
      data: {
        newGameCode,
        initiatedByPlayerName
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // User accepted, navigate to join page
        this.router.navigate(['/games/join', newGameCode]);
      }
    });
  }
}
