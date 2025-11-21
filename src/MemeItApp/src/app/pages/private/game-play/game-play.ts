import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormsModule } from '@angular/forms';
import { Subscription, interval } from 'rxjs';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { MemeService } from '@services/meme.service';
import { MemeTemplate, TextAreaDefinition } from '@models/meme.model';

@Component({
    selector: 'memeit-game-play',
    imports: [
        CommonModule,
        FormsModule,
        MatButtonModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatProgressSpinnerModule,
        MatProgressBarModule
    ],
    templateUrl: './game-play.html',
    styleUrl: './game-play.scss',
})
export class GamePlayPage implements OnInit, OnDestroy {
    gameCode: string = '';
    roundNumber: number = 1;
    roundStartedAt: Date | null = null;
    memeTemplate: MemeTemplate | null = null;
    textInputs: string[] = [];
    isLoading = true;
    errorMessage = '';
    memeChangesRemaining = 2;
    timeRemaining = 30;
    progressPercentage = 100;
    private subscriptions: Subscription[] = [];
    private timerSubscription?: Subscription;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private gameService: GameService,
        private memeService: MemeService,
        private notificationService: NotificationService
    ) { }

    ngOnInit(): void {
        this.gameCode = this.route.snapshot.paramMap.get('code') || '';

        if (!this.gameCode) {
            this.router.navigate(['/']);
            return;
        }

        // Load the player's round state first
        this.loadPlayerRoundState();
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(sub => sub.unsubscribe());
        if (this.timerSubscription) {
            this.timerSubscription.unsubscribe();
        }
    }

    private loadPlayerRoundState(): void {
        this.isLoading = true;

        this.gameService.getPlayerRoundState(this.gameCode).subscribe({
            next: (state) => {
                this.roundNumber = state.roundNumber;
                this.roundStartedAt = new Date(state.roundStartedAt);

                // Start the timer
                this.startTimer();

                // If player already has a meme selected, load it
                if (state.selectedMemeTemplateId) {
                    this.loadMemeById(state.selectedMemeTemplateId);
                } else {
                    // Otherwise, load a random meme
                    this.loadRandomMeme();
                }
            },
            error: (error) => {
                console.error('Failed to load player round state:', error);
                this.isLoading = false;
                this.errorMessage = 'Failed to load game state. Please try again.';
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

    private loadMemeById(memeId: string): void {
        this.memeService.getTemplateById(memeId).subscribe({
            next: (template) => {
                this.memeTemplate = template;
                this.textInputs = new Array(template.textAreas.length).fill('');
                this.isLoading = false;
                this.memeChangesRemaining = 0; // Already selected, no changes allowed
            },
            error: (error: any) => {
                console.error('Failed to load meme by ID:', error);
                // Fallback to random meme
                this.loadRandomMeme();
            }
        });
    }

    private loadRandomMeme(): void {
        this.isLoading = true;

        this.memeService.getRandomTemplate().subscribe({
            next: (template) => {
                this.memeTemplate = template;
                this.textInputs = new Array(template.textAreas.length).fill('');
                this.isLoading = false;

                // Select this meme template for the round
                if (template.id) {
                    this.gameService.selectMemeTemplate(this.gameCode, this.roundNumber, template.id).subscribe({
                        next: () => {
                            console.log('Meme template selected successfully');
                        },
                        error: (error) => {
                            console.error('Failed to select meme template:', error);
                        }
                    });
                }
            },
            error: (error) => {
                console.error('Failed to load random meme:', error);
                this.isLoading = false;
                this.errorMessage = 'Failed to load meme template. Please try again.';
                this.notificationService.error('Error', 'Failed to load meme template');
            }
        });
    }

    selectNewMeme(): void {
        if (this.memeChangesRemaining > 0) {
            this.memeChangesRemaining--;
            this.loadRandomMeme();

            if (this.memeChangesRemaining === 0) {
                this.notificationService.info('Notice', 'This is your last meme selection!');
            }
        }
    }

    get canSelectNewMeme(): boolean {
        return this.memeChangesRemaining > 0;
    }

    submitMeme(): void {
        if (!this.memeTemplate) return;

        this.gameService.selectMemeTemplate(this.gameCode, this.roundNumber, this.memeTemplate.id).subscribe({
            next: () => {
                this.notificationService.success('Success', 'Meme submitted successfully!');
                // TODO: Navigate to next phase or wait for other players
            },
            error: (error) => {
                console.error('Failed to submit meme:', error);
                this.notificationService.error('Error', 'Failed to submit meme. Please try again.');
            }
        });
    }
}
