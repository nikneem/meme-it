import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subscription, interval } from 'rxjs';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { RealtimeService } from '@services/realtime.service';
import { MemeCreativeComponent } from '@components/meme-creative/meme-creative.component';
import { MemeRatingComponent, MemeSubmission } from '@components/meme-rating/meme-rating.component';

@Component({
    selector: 'memeit-game-play',
    imports: [
        CommonModule,
        MatProgressBarModule,
        MemeCreativeComponent,
        MemeRatingComponent
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
    currentPhase: 'creative' | 'score' = 'creative';
    currentMemeToRate: MemeSubmission | null = null;
    private subscriptions: Subscription[] = [];
    private timerSubscription?: Subscription;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private gameService: GameService,
        private notificationService: NotificationService,
        private realtimeService: RealtimeService
    ) { }

    ngOnInit(): void {
        this.gameCode = this.route.snapshot.paramMap.get('code') || '';

        if (!this.gameCode) {
            this.router.navigate(['/']);
            return;
        }

        const creativePhaseEndedSub = this.realtimeService.creativePhaseEnded$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCode) {
                    this.onCreativePhaseEnded(event.roundNumber);
                }
            }
        });
        this.subscriptions.push(creativePhaseEndedSub);

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
                this.currentPhase = 'creative';
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
        const roundDuration = 30;
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
        this.notificationService.info('Time is Up!', 'The round has ended.');
    }

    onMemeSubmitted(): void {
        console.log('Meme submitted from creative component');
    }

    onCreativePhaseEnded(roundNumber: number): void {
        console.log('Creative phase ended, switching to score phase');
        this.currentPhase = 'score';
        this.loadNextMemeToRate();
    }

    onRatingSubmitted(): void {
        console.log('Rating submitted from rating component');
        this.loadNextMemeToRate();
    }

    private loadNextMemeToRate(): void {
        this.gameService.getNextMemeToScore(this.gameCode, this.roundNumber).subscribe({
            next: (meme) => {
                if (meme) {
                    this.currentMemeToRate = meme;
                } else {
                    this.currentMemeToRate = null;
                    this.notificationService.info('Round Complete', 'All memes have been rated!');
                }
            },
            error: (error) => {
                console.error('Failed to load next meme to rate:', error);
                this.currentMemeToRate = null;
            }
        });
    }

    get isCreativePhase(): boolean {
        return this.currentPhase === 'creative';
    }

    get isScorePhase(): boolean {
        return this.currentPhase === 'score';
    }
}