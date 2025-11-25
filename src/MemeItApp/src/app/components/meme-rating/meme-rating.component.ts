import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { GameService } from '@services/game.service';
import { NotificationService } from '@services/notification.service';

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
export class MemeRatingComponent implements OnInit {
    @Input() gameCode: string = '';
    @Input() roundNumber: number = 1;
    @Input() memeSubmission: MemeSubmission | null = null;
    @Output() ratingSubmitted = new EventEmitter<void>();

    rating: number = 0;
    hoverRating: number = 0;
    isSubmitting: boolean = false;

    constructor(
        private gameService: GameService,
        private notificationService: NotificationService
    ) { }

    ngOnInit(): void {
        // Component initialization
    }

    setRating(rating: number): void {
        this.rating = rating;
    }

    setHoverRating(rating: number): void {
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
        if (!this.memeSubmission || this.rating === 0) {
            this.notificationService.info('Invalid Rating', 'Please select a rating before submitting.');
            return;
        }

        this.isSubmitting = true;

        this.gameService.rateMeme(this.gameCode, this.roundNumber, this.memeSubmission.id, this.rating).subscribe({
            next: () => {
                this.notificationService.success('Success', 'Rating submitted successfully!');
                this.ratingSubmitted.emit();
            },
            error: (error) => {
                console.error('Failed to submit rating:', error);
                this.notificationService.error('Error', 'Failed to submit rating. Please try again.');
                this.isSubmitting = false;
            }
        });
    }
}
