import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
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
        MatProgressSpinnerModule
    ],
    templateUrl: './game-play.html',
    styleUrl: './game-play.scss',
})
export class GamePlayPage implements OnInit, OnDestroy {
    gameCode: string = '';
    roundNumber: number = 1;
    memeTemplate: MemeTemplate | null = null;
    textInputs: string[] = [];
    isLoading = true;
    errorMessage = '';
    memeChangesRemaining = 2;
    private subscriptions: Subscription[] = [];

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

        // TODO: Get current round number from game state
        this.roundNumber = 1;

        this.loadRandomMeme();
    }

    ngOnDestroy(): void {
        this.subscriptions.forEach(sub => sub.unsubscribe());
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
