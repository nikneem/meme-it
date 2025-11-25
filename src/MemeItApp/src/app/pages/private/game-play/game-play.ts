import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { RealtimeService } from '@services/realtime.service';
import { MemeCreativeComponent } from '@components/meme-creative/meme-creative.component';
import { MemeRatingComponent, MemeSubmission } from '@components/meme-rating/meme-rating.component';
import { GameTimerComponent } from '@components/game-timer/game-timer.component';

@Component({
    selector: 'memeit-game-play',
    imports: [
        CommonModule,
        MemeCreativeComponent,
        MemeRatingComponent,
        GameTimerComponent
    ],
    templateUrl: './game-play.html',
    styleUrl: './game-play.scss',
})
export class GamePlayPage implements OnInit, OnDestroy {
    gameCode: string = '';
    roundNumber: number = 1;
    roundStartedAt: Date | null = null;
    timerDuration = 0;
    currentPhase: 'creative' | 'score' = 'creative';
    currentMemeToRate: MemeSubmission | null = null;
    private subscriptions: Subscription[] = [];

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
    }

    private loadPlayerRoundState(): void {
        this.gameService.getPlayerRoundState(this.gameCode).subscribe({
            next: (state) => {
                this.roundNumber = state.roundNumber;
                this.roundStartedAt = new Date(state.roundStartedAt);
                this.currentPhase = 'creative';
                this.calculateTimerDuration();
            },
            error: (error) => {
                console.error('Failed to load player round state:', error);
                this.notificationService.error('Error', 'Failed to load game state');
            }
        });
    }

    private calculateTimerDuration(): void {
        if (!this.roundStartedAt) {
            this.timerDuration = 0;
            return;
        }
        const roundDuration = 30;
        const now = new Date();
        const elapsed = (now.getTime() - this.roundStartedAt.getTime()) / 1000;
        this.timerDuration = Math.max(0, roundDuration - elapsed);
    }

    onTimeExpired(): void {
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