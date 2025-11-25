import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subscription, interval } from 'rxjs';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { MemeCreativeComponent } from '@components/meme-creative/meme-creative.component';

@Component({
    selector: 'memeit-game-play',
    imports: [
        CommonModule,
        MatProgressBarModule,
        MemeCreativeComponent
    ],
    templateUrl: './game-play.html',
    styleUrl: './game-play.scss',
})
export class GamePlayPage implements OnInit, OnDestroy {
    gameCode: string = '';
    roundNumber: number = 1;
    roundStartedAt: Date | null = null;
    timeRemaining = 30;
    progressPercentage = 100;
    private subscriptions: Subscription[] = [];
    private timerSubscription?: Subscription;

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

        // Load the player's round state to get round number and start time
        this.loadPlayerRoundState();
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(sub => sub.unsubscribe());
        if (this.timerSubscription) {
            this.timerSubscription.unsubscribe();
        }
    }

    private loadPlayerRoundState(): void {
        this.gameService.getPlayerRoundState(this.gameCode).subscribe({
            next: (state) => {
                this.roundNumber = state.roundNumber;
                this.roundStartedAt = new Date(state.roundStartedAt);

                // Start the timer
                this.startTimer();
            },
            error: (error) => {
                console.error('Failed to load player round state:', error);
                this.notificationService.error('Error', 'Failed to load game state');
            }
        });
    }

    private startTimer(): void {
        if (!this.roundStartedAt) return;

        const roundDuration = 30; // 30 seconds

        // Update timer every 100ms for smooth progress bar
        this.timerSubscription = interval(100).subscribe(() => {
            const now = new Date();
            const elapsed = (now.getTime() - this.roundStartedAt!.getTime()) / 1000;
            this.timeRemaining = Math.max(0, roundDuration - elapsed);
            this.progressPercentage = (this.timeRemaining / roundDuration) * 100;

            if (this.timeRemaining <= 0) {
                this.onTimeExpired();
            }
        });
    }

    private onTimeExpired(): void {
        if (this.timerSubscription) {
            this.timerSubscription.unsubscribe();
        }
        this.notificationService.info('Time\'s Up!', 'The round has ended.');
        // TODO: Auto-submit or navigate to next phase
    }

    onMemeSubmitted(): void {
        // Handle meme submission - could navigate to waiting screen or next phase
        console.log('Meme submitted from creative component');
        // TODO: Navigate to next phase or wait for other players
    }
}
