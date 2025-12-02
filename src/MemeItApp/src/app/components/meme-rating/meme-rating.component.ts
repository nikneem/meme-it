import { Component, Input, OnInit, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';
import { AuthService } from '@services/auth.service';

export interface MemeSubmission {
    id: string;
    memeTemplateId: string;
    imageUrl: string;
    width: number;
    height: number;
    textEntries: MemeTextEntry[];
    createdBy: string;
    createdByName: string;
}

export interface MemeTextEntry {
    x: number;
    y: number;
    width: number;
    height: number;
    text: string;
    fontSize: number;
    fontColor: string;
    borderSize: number;
    borderColor: string;
    isBold: boolean;
}

@Component({
    selector: 'memeit-meme-rating',
    imports: [
        CommonModule,
        MatButtonModule,
        MatCardModule,
        MatIconModule
    ],
    templateUrl: './meme-rating.component.html',
    styleUrl: './meme-rating.component.scss',
})
export class MemeRatingComponent implements OnInit, OnChanges {
    @Input() gameCode: string = '';
    @Input() roundNumber: number = 1;
    @Input() memeSubmission: MemeSubmission | null = null;
    @Output() ratingSubmitted = new EventEmitter<void>();

    rating: number = 0;
    hoverRating: number = 0;
    isSubmitting: boolean = false;
    hasSubmitted: boolean = false;
    isOwnMeme: boolean = false;

    constructor(
        private gameService: GameService,
        private notificationService: NotificationService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        this.checkMemeOwnership();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['memeSubmission'] && this.memeSubmission) {
            // Reset state when a new meme arrives
            this.rating = 0;
            this.hoverRating = 0;
            this.hasSubmitted = false;
            this.isSubmitting = false;
            this.checkMemeOwnership();
        }
    }

    private checkMemeOwnership(): void {
        const currentUserId = this.authService.getCurrentUserId();
        this.isOwnMeme = this.memeSubmission?.createdBy === currentUserId;

        if (this.isOwnMeme) {
            this.notificationService.info('Your Meme', 'This is your own meme. You cannot rate it.');
        }
    }

    setRating(rating: number): void {
        if (this.isOwnMeme || this.hasSubmitted) {
            return;
        }
        this.rating = rating;
    }

    setHoverRating(rating: number): void {
        if (this.isOwnMeme || this.hasSubmitted) {
            return;
        }
        this.hoverRating = rating;
    }

    clearHoverRating(): void {
        this.hoverRating = 0;
    }

    getStarIcon(index: number): string {
        const currentRating = this.hoverRating || this.rating;
        return index <= currentRating ? 'star' : 'star_border';
    }

    submitRating(): void {
        if (this.isOwnMeme) {
            this.notificationService.info('Cannot Rate', 'You cannot rate your own meme.');
            return;
        }

        if (!this.memeSubmission || this.rating === 0) {
            this.notificationService.info('Invalid Rating', 'Please select a rating before submitting.');
            return;
        }

        this.isSubmitting = true;

        this.gameService.rateMeme(this.gameCode, this.roundNumber, this.memeSubmission.id, this.rating).subscribe({
            next: () => {
                this.notificationService.success('Success', 'Rating submitted successfully!');
                // Mark as submitted and keep the meme visible
                this.hasSubmitted = true;
                this.isSubmitting = false;
                this.ratingSubmitted.emit();
            },
            error: (error) => {
                console.error('Failed to submit rating:', error);
                this.notificationService.error('Error', 'Failed to submit rating. Please try again.');
                this.isSubmitting = false;
            }
        });
    }

    get canRate(): boolean {
        return !this.isOwnMeme && !this.hasSubmitted;
    }

    wrapText(text: string, maxWidth: number, fontSize: number): string[] {
        if (!text) return [''];

        // Approximate character width based on font size (roughly 0.6 of fontSize for average characters)
        const avgCharWidth = fontSize * 0.6;
        const maxCharsPerLine = Math.floor(maxWidth / avgCharWidth);

        if (maxCharsPerLine <= 0) return [text];

        const words = text.split(' ');
        const lines: string[] = [];
        let currentLine = '';

        for (const word of words) {
            const testLine = currentLine ? `${currentLine} ${word}` : word;

            if (testLine.length <= maxCharsPerLine) {
                currentLine = testLine;
            } else {
                if (currentLine) {
                    lines.push(currentLine);
                }
                // If single word is too long, break it
                if (word.length > maxCharsPerLine) {
                    let remainingWord = word;
                    while (remainingWord.length > maxCharsPerLine) {
                        lines.push(remainingWord.substring(0, maxCharsPerLine));
                        remainingWord = remainingWord.substring(maxCharsPerLine);
                    }
                    currentLine = remainingWord;
                } else {
                    currentLine = word;
                }
            }
        }

        if (currentLine) {
            lines.push(currentLine);
        }

        return lines.length > 0 ? lines : [''];
    }
}
