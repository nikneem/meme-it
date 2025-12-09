import { Component, OnInit, OnDestroy, ViewChild, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { CanComponentDeactivate } from '../../../guards/can-deactivate-game.guard';
import { GameService } from '@services/game.service';
import { MemeService } from '@services/meme.service';
import { NotificationService } from '@services/notification.service';
import { RealtimeService } from '@services/realtime.service';
import { MemeCreativeComponent } from '@components/meme-creative/meme-creative.component';
import { MemeRatingComponent, MemeSubmission as ComponentMemeSubmission } from '@components/meme-rating/meme-rating.component';
import { GameTimerComponent } from '@components/game-timer/game-timer.component';
import { GameScoreboardComponent } from '@components/game-scoreboard/game-scoreboard.component';
import { GameStore } from '../../../store/game.store';

@Component({
    selector: 'memeit-game-play',
    imports: [
        CommonModule,
        MemeCreativeComponent,
        MemeRatingComponent,
        GameTimerComponent,
        GameScoreboardComponent
    ],
    templateUrl: './game-play.html',
    styleUrl: './game-play.scss',
})
export class GamePlayPage implements OnInit, OnDestroy, CanComponentDeactivate {
    @ViewChild(GameTimerComponent) gameTimer?: GameTimerComponent;

    // Inject the game store
    readonly gameStore = inject(GameStore);

    // Store signals for template binding
    readonly gameCode = this.gameStore.gameCode;
    readonly currentRound = this.gameStore.currentRound;
    readonly totalRounds = this.gameStore.totalRounds;
    readonly currentPhase = this.gameStore.currentPhase;
    readonly timer = this.gameStore.timer;
    readonly showScoreboard = this.gameStore.showScoreboard;
    readonly finalScores = this.gameStore.finalScores;
    readonly timeRemaining = this.gameStore.timeRemaining;
    readonly isCreativePhase = this.gameStore.isCreativePhase;
    readonly isScoringPhase = this.gameStore.isScoringPhase;
    readonly isAdmin = this.gameStore.isAdmin;

    // Local state for enriched meme submission
    currentMemeToRate: ComponentMemeSubmission | null = null;

    private subscriptions: Subscription[] = [];
    private gameCodeValue: string = '';

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private gameService: GameService,
        private memeService: MemeService,
        private notificationService: NotificationService,
        private realtimeService: RealtimeService
    ) {
        // Watch for meme to rate changes and enrich with template data
        effect(() => {
            const storeMeme = this.gameStore.currentMemeToRate();
            if (storeMeme) {
                this.enrichMemeSubmission(storeMeme);
            } else {
                this.currentMemeToRate = null;
            }
        });
    }

    async ngOnInit(): Promise<void> {
        this.gameCodeValue = this.route.snapshot.paramMap.get('code') || '';

        if (!this.gameCodeValue) {
            this.router.navigate(['/']);
            return;
        }

        // Initialize the game store - this will restore from session storage if available
        this.gameStore.initializeGame(this.gameCodeValue);

        // Connect to SignalR and join the game group
        try {
            await this.realtimeService.connect();
            await this.realtimeService.joinGameGroup(this.gameCodeValue);
        } catch (error) {
            console.error('Failed to connect to real-time service:', error);
            this.notificationService.error('Connection Error', 'Failed to connect to real-time updates');
        }

        // Subscribe to SignalR events and update store
        const roundStartedSub = this.realtimeService.roundStarted$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCodeValue) {
                    this.gameStore.handleRoundStarted(event);
                    this.notificationService.success(
                        'New Round Started',
                        `Round ${event.roundNumber} has begun!`,
                        undefined,
                        3000
                    );
                }
            }
        });
        this.subscriptions.push(roundStartedSub);

        const creativePhaseEndedSub = this.realtimeService.creativePhaseEnded$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCodeValue) {
                    this.gameStore.handleCreativePhaseEnded(event);
                    console.log('Creative phase ended, waiting for score phase');
                }
            }
        });
        this.subscriptions.push(creativePhaseEndedSub);

        const scorePhaseStartedSub = this.realtimeService.scorePhaseStarted$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCodeValue) {
                    this.gameStore.handleScorePhaseStarted(event);
                    console.log('Score phase started, meme ready for rating');
                }
            }
        });
        this.subscriptions.push(scorePhaseStartedSub);

        const roundEndedSub = this.realtimeService.roundEnded$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCodeValue) {
                    this.gameStore.handleRoundEnded(event);

                    // Reset timer
                    if (this.gameTimer) {
                        this.gameTimer.resetTimer();
                    }

                    // Display notification
                    if (event.roundNumber >= event.totalRounds) {
                        this.notificationService.success(
                            'Game Complete!',
                            'All rounds finished. Check out the final scores!',
                            undefined,
                            8000
                        );
                    } else {
                        this.notificationService.info(
                            'Round Complete',
                            `Round ${event.roundNumber} has ended. Next round starting soon!`,
                            undefined,
                            5000
                        );
                    }
                }
            }
        });
        this.subscriptions.push(roundEndedSub);

        const gameStartedSub = this.realtimeService.gameStarted$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCodeValue) {
                    this.gameStore.handleGameStarted(event);
                    this.notificationService.success(
                        'Game Started!',
                        `Round ${event.roundNumber} is beginning!`,
                        undefined,
                        3000
                    );
                }
            }
        });
        this.subscriptions.push(gameStartedSub);
    }

    async ngOnDestroy(): Promise<void> {
        this.subscriptions.forEach(sub => sub.unsubscribe());

        // Leave the game group but keep connection alive for potential navigation back
        if (this.gameCodeValue) {
            try {
                await this.realtimeService.leaveGameGroup(this.gameCodeValue);
            } catch (error) {
                console.error('Failed to leave game group:', error);
            }
        }
    }

    onTimeExpired(): void {
        this.notificationService.info('Time is Up!', 'The round has ended.');
    }

    onMemeSubmitted(): void {
        console.log('Meme submitted from creative component');
    }

    onRatingSubmitted(): void {
        console.log('Rating submitted from rating component');
    }

    private enrichMemeSubmission(storeMeme: any): void {
        // Fetch template details to build complete MemeSubmission for component
        this.memeService.getTemplateById(storeMeme.memeTemplateId).subscribe({
            next: (template) => {
                this.currentMemeToRate = {
                    id: storeMeme.memeId,
                    memeTemplateId: storeMeme.memeTemplateId,
                    imageUrl: template.imageUrl,
                    width: template.width || 800,
                    height: template.height || 600,
                    textEntries: template.textAreas.map((textArea: any, index: number) => {
                        const playerText = storeMeme.textEntries[index]?.value || '';
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
                    createdBy: storeMeme.playerId,
                    createdByName: storeMeme.playerName
                };
            },
            error: (error) => {
                console.error('Failed to load meme template:', error);
                this.notificationService.error('Error', 'Failed to load meme for rating');
            }
        });
    }

    get isScoreboardPhase(): boolean {
        return this.currentPhase() === 'ended' && this.showScoreboard();
    }

    getTimerEndDate(): Date | null {
        const endTime = this.timer().endTime;
        return endTime ? new Date(endTime) : null;
    }

    onStartNewGame(): void {
        const currentGameCode = this.gameCode();
        if (!currentGameCode) {
            this.notificationService.error('Error', 'Game code not found');
            return;
        }

        this.realtimeService.leaveGameGroup(currentGameCode);

        this.gameService.createGame({ previousGameCode: currentGameCode }).subscribe({
            next: async (response) => {
                this.notificationService.success(
                    'New Game Created!',
                    `Game ${response.gameCode} created. All players have been invited!`,
                    undefined,
                    5000
                );

                // Navigate to the new game lobby
                this.router.navigate(['/app/games/', response.gameCode]);
            },
            error: (error) => {
                console.error('Failed to create new game:', error);
                this.notificationService.error(
                    'Failed to Create Game',
                    'Could not start a new game. Please try again.'
                );
            }
        });
    }

    canDeactivate(): boolean {
        // Allow navigation only when round has ended (not during active creative/scoring phases)
        const phase = this.currentPhase();
        return phase === 'ended';
    }
}
