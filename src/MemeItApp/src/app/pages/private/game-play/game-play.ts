import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { GameService } from '@services/game.service';
import { MemeService } from '@services/meme.service';
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
    creativePhaseEndTime: Date | null = null;
    timerDuration = 0;
    currentPhase: 'creative' | 'score' = 'creative';
    currentMemeToRate: MemeSubmission | null = null;
    private subscriptions: Subscription[] = [];

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private gameService: GameService,
        private memeService: MemeService,
        private notificationService: NotificationService,
        private realtimeService: RealtimeService
    ) { }

    async ngOnInit(): Promise<void> {
        this.gameCode = this.route.snapshot.paramMap.get('code') || '';

        if (!this.gameCode) {
            this.router.navigate(['/']);
            return;
        }

        // Connect to SignalR and join the game group
        try {
            await this.realtimeService.connect();
            await this.realtimeService.joinGameGroup(this.gameCode);
        } catch (error) {
            console.error('Failed to connect to real-time service:', error);
            this.notificationService.error('Connection Error', 'Failed to connect to real-time updates');
        }

        const creativePhaseEndedSub = this.realtimeService.creativePhaseEnded$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCode) {
                    this.onCreativePhaseEnded(event.roundNumber);
                }
            }
        });
        this.subscriptions.push(creativePhaseEndedSub);

        const scorePhaseStartedSub = this.realtimeService.scorePhaseStarted$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCode) {
                    this.onScorePhaseStarted(event);
                }
            }
        });
        this.subscriptions.push(scorePhaseStartedSub);

        this.loadPlayerRoundState();
    }

    async ngOnDestroy(): Promise<void> {
        this.subscriptions.forEach(sub => sub.unsubscribe());

        // Leave the game group but keep connection alive for potential navigation back
        if (this.gameCode) {
            try {
                await this.realtimeService.leaveGameGroup(this.gameCode);
            } catch (error) {
                console.error('Failed to leave game group:', error);
            }
        }
    }

    private loadPlayerRoundState(): void {
        this.gameService.getPlayerRoundState(this.gameCode).subscribe({
            next: (state) => {
                this.roundNumber = state.roundNumber;
                this.roundStartedAt = new Date(state.roundStartedAt);
                this.creativePhaseEndTime = new Date(state.creativePhaseEndTime);
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
        console.log('Creative phase ended, waiting for score phase to start');
        this.currentPhase = 'score';
    }

    onScorePhaseStarted(event: any): void {
        console.log('Score phase started, loading meme for rating:', event);
        this.currentPhase = 'score';

        // Update timer with rating duration from event
        this.timerDuration = event.ratingDurationSeconds || 30;

        // Fetch the meme template details to build the complete MemeSubmission
        this.memeService.getTemplateById(event.memeTemplateId).subscribe({
            next: (template) => {
                // Map the event data to MemeSubmission format
                // Combine template structure with player's text entries
                this.currentMemeToRate = {
                    id: event.memeId,
                    memeTemplateId: event.memeTemplateId,
                    imageUrl: template.imageUrl,
                    width: template.width || 800,
                    height: template.height || 600,
                    textEntries: template.textAreas.map((textArea, index) => {
                        // Find the corresponding text entry from the event
                        const playerText = event.textEntries[index]?.value || '';
                        return {
                            x: textArea.x,
                            y: textArea.y,
                            width: textArea.width,
                            height: textArea.height,
                            text: playerText,
                            fontSize: textArea.fontSize,
                            fontColor: textArea.fontColor,
                            borderSize: textArea.borderSize,
                            borderColor: textArea.borderColor,
                            isBold: textArea.isBold
                        };
                    }),
                    createdBy: event.playerId,
                    createdByName: 'Player' // We don't have the player name in the event
                };
            },
            error: (error) => {
                console.error('Failed to load meme template:', error);
                this.notificationService.error('Error', 'Failed to load meme for rating');
            }
        });
    }

    onRatingSubmitted(): void {
        console.log('Rating submitted from rating component');
        // Clear current meme and wait for next ScorePhaseStarted event
        this.currentMemeToRate = null;
    }

    get isCreativePhase(): boolean {
        return this.currentPhase === 'creative';
    }

    get isScorePhase(): boolean {
        return this.currentPhase === 'score';
    }
}