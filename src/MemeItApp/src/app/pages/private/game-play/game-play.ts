import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
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
import { GameScoreboardComponent } from '@components/game-scoreboard/game-scoreboard.component';

interface ScoreboardEntry {
    playerId: string;
    playerName: string;
    totalScore: number;
}

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
export class GamePlayPage implements OnInit, OnDestroy {
    @ViewChild(GameTimerComponent) gameTimer?: GameTimerComponent;

    gameCode: string = '';
    roundNumber: number = 1;
    totalRounds: number = 5;
    roundStartedAt: Date | null = null;
    creativePhaseEndTime: Date | null = null;
    timerDuration = 0;
    currentPhase: 'creative' | 'score' | 'scoreboard' = 'creative';
    currentMemeToRate: MemeSubmission | null = null;
    currentScoreboard: ScoreboardEntry[] = [];
    showScoreboard = false;
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

        const roundStartedSub = this.realtimeService.roundStarted$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCode) {
                    this.onRoundStarted(event.roundNumber, event.durationInSeconds);
                }
            }
        });
        this.subscriptions.push(roundStartedSub);

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

        const roundEndedSub = this.realtimeService.roundEnded$.subscribe({
            next: (event) => {
                if (event.gameCode === this.gameCode) {
                    this.onRoundEnded(event);
                }
            }
        });
        this.subscriptions.push(roundEndedSub);

        // Load the complete game state from the server
        this.loadGameState();
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

    private loadGameState(): void {
        // Fetch comprehensive game state including current round and player submission info
        this.gameService.refreshGame(this.gameCode).subscribe({
            next: (game) => {
                // Update total rounds from game configuration or default
                this.totalRounds = game.rounds?.length || 5;

                // Check if game is in progress and has current round info
                if (game.state === 'InProgress' && game.currentRoundInfo) {
                    // Restore complete game state
                    this.restoreGameState(game);
                } else if (game.state === 'Completed') {
                    this.notificationService.info(
                        'Game Ended',
                        'This game has already completed.',
                        undefined,
                        5000
                    );
                    // Could navigate back to lobby or show final scores
                } else if (game.state === 'Scoring' && game.currentRoundInfo) {
                    // Game is in scoring phase
                    this.restoreGameState(game);
                } else {
                    this.notificationService.info(
                        'Game Not Started',
                        'Waiting for the game to start...',
                        undefined,
                        3000
                    );
                }
            },
            error: (error) => {
                console.error('Failed to load game state:', error);
                this.notificationService.error('Error', 'Failed to load game information');
                // Optionally navigate back
                setTimeout(() => this.router.navigate(['/']), 3000);
            }
        });
    }

    private restoreGameState(game: any): void {
        const currentRound = game.currentRoundInfo;

        // Set round information
        this.roundNumber = currentRound.roundNumber;
        this.roundStartedAt = new Date(currentRound.startedAt);

        if (currentRound.creativePhaseEndTime) {
            this.creativePhaseEndTime = new Date(currentRound.creativePhaseEndTime);
        }

        // Determine and set current phase
        const now = new Date();

        if (currentRound.phase === 'Creative') {
            this.currentPhase = 'creative';
            this.calculateTimerDuration();

            // If player has already submitted, they might be waiting
            if (game.playerSubmission) {
                console.log('Player has already submitted meme for this round');
            }
        } else if (currentRound.phase === 'Scoring') {
            this.currentPhase = 'score';
            // Try to fetch next meme to rate
            this.tryFetchNextMemeToRate();
        } else if (currentRound.phase === 'Ended') {
            this.currentPhase = 'scoreboard';
            // Round has ended, wait for scoreboard or next round
            console.log('Round has ended, waiting for scoreboard');
        }

        console.log('Game state restored:', {
            round: this.roundNumber,
            phase: this.currentPhase,
            hasSubmission: !!game.playerSubmission
        });
    }

    private loadPlayerRoundState(): void {
        this.gameService.getPlayerRoundState(this.gameCode).subscribe({
            next: (state) => {
                this.roundNumber = state.roundNumber;
                this.roundStartedAt = new Date(state.roundStartedAt);
                this.creativePhaseEndTime = new Date(state.creativePhaseEndTime);

                // Determine current phase based on timing
                const now = new Date();
                if (now < this.creativePhaseEndTime) {
                    this.currentPhase = 'creative';
                    this.calculateTimerDuration();
                } else {
                    // Creative phase has ended, we're in scoring/waiting phase
                    this.currentPhase = 'score';
                    // Wait for score phase started event or try to fetch next meme
                    this.tryFetchNextMemeToRate();
                }
            },
            error: (error) => {
                console.error('Failed to load player round state:', error);
                this.notificationService.error('Error', 'Failed to load game state');
            }
        });
    }

    private tryFetchNextMemeToRate(): void {
        if (!this.gameCode || !this.roundNumber) {
            return;
        }

        this.gameService.getNextMemeToScore(this.gameCode, this.roundNumber).subscribe({
            next: (memeData) => {
                if (memeData && memeData.memeId) {
                    // We have a meme to rate
                    this.currentPhase = 'score';
                    // The meme data structure might need to be mapped similar to onScorePhaseStarted
                    console.log('Fetched meme to rate:', memeData);
                } else {
                    // No more memes to rate, possibly waiting for scoreboard
                    console.log('No memes available to rate, waiting for next phase');
                }
            },
            error: (error) => {
                console.log('No memes currently available to rate:', error);
                // This is okay - we'll wait for real-time events
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

    onRoundStarted(roundNumber: number, durationInSeconds: number): void {
        console.log('New round started:', roundNumber, 'Duration:', durationInSeconds);
        this.notificationService.success(
            'New Round Started',
            `Round ${roundNumber} has begun!`,
            undefined,
            3000
        );

        // Hide scoreboard and reset to creative phase for the new round
        this.showScoreboard = false;
        this.roundNumber = roundNumber;
        this.currentPhase = 'creative';
        this.currentMemeToRate = null;
        this.roundStartedAt = new Date();

        // Set timer for creative phase with duration from event
        this.timerDuration = durationInSeconds;

        // Reload player round state to get updated information
        this.loadPlayerRoundState();
    }

    onCreativePhaseEnded(roundNumber: number): void {
        console.log('Creative phase ended, waiting for score phase to start');
        this.currentPhase = 'score';
    }

    onRoundEnded(event: any): void {
        console.log('Round ended, showing scoreboard:', event);

        // Reset the timer to stop it from running
        if (this.gameTimer) {
            this.gameTimer.resetTimer();
        }

        // Update scoreboard data
        this.currentScoreboard = event.scoreboard || [];
        this.totalRounds = event.totalRounds || 5;
        this.showScoreboard = true;
        this.currentPhase = 'scoreboard';

        // Display notification based on whether it's the final round
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

    get isScoreboardPhase(): boolean {
        return this.currentPhase === 'scoreboard' && this.showScoreboard;
    }
}