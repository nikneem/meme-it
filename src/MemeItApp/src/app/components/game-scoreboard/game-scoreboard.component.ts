import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { trigger, transition, style, animate, query, stagger } from '@angular/animations';

export interface ScoreboardEntry {
    playerId: string;
    playerName: string;
    totalScore: number;
}

@Component({
    selector: 'memeit-game-scoreboard',
    standalone: true,
    imports: [CommonModule, MatCardModule, MatIconModule],
    templateUrl: './game-scoreboard.component.html',
    styleUrl: './game-scoreboard.component.scss',
    animations: [
        trigger('fadeInScale', [
            transition(':enter', [
                style({ opacity: 0, transform: 'scale(0.8)' }),
                animate('400ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
            ])
        ]),
        trigger('listAnimation', [
            transition('* => *', [
                query(':enter', [
                    style({ opacity: 0, transform: 'translateY(20px)' }),
                    stagger(100, [
                        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
                    ])
                ], { optional: true })
            ])
        ]),
        trigger('winnerCelebration', [
            transition(':enter', [
                style({ opacity: 0, transform: 'scale(0.5) rotate(-10deg)' }),
                animate('600ms cubic-bezier(0.68, -0.55, 0.265, 1.55)',
                    style({ opacity: 1, transform: 'scale(1) rotate(0deg)' }))
            ])
        ])
    ]
})
export class GameScoreboardComponent {
    @Input() roundNumber: number = 1;
    @Input() totalRounds: number = 5;
    @Input() scoreboard: ScoreboardEntry[] = [];

    protected get isFinalRound(): boolean {
        return this.roundNumber >= this.totalRounds;
    }

    protected get title(): string {
        return this.isFinalRound
            ? 'Final Results'
            : `Round ${this.roundNumber} of ${this.totalRounds}`;
    }

    protected get winner(): ScoreboardEntry | null {
        if (!this.scoreboard || this.scoreboard.length === 0) {
            return null;
        }
        return this.scoreboard[0];
    }

    protected get sortedScoreboard(): ScoreboardEntry[] {
        if (!this.scoreboard) {
            return [];
        }
        return [...this.scoreboard].sort((a, b) => b.totalScore - a.totalScore);
    }

    protected getRankIcon(index: number): string {
        switch (index) {
            case 0: return 'emoji_events'; // Gold trophy
            case 1: return 'military_tech'; // Silver medal
            case 2: return 'workspace_premium'; // Bronze badge
            default: return 'stars';
        }
    }

    protected getRankClass(index: number): string {
        switch (index) {
            case 0: return 'rank-first';
            case 1: return 'rank-second';
            case 2: return 'rank-third';
            default: return 'rank-other';
        }
    }
}
