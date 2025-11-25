import { Component, Input, Output, EventEmitter, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { Subscription, interval } from 'rxjs';

@Component({
    selector: 'memeit-game-timer',
    imports: [CommonModule, MatProgressBarModule],
    templateUrl: './game-timer.component.html',
    styleUrl: './game-timer.component.scss',
})
export class GameTimerComponent implements OnDestroy, OnChanges {
    @Input() duration: number = 0; // Duration in seconds
    @Output() timeExpired = new EventEmitter<void>();

    timeRemaining = 0;
    progressPercentage = 100;
    private timerSubscription?: Subscription;

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['duration']) {
            this.startTimer();
        }
    }

    ngOnDestroy(): void {
        this.stopTimer();
    }

    private startTimer(): void {
        this.stopTimer();

        if (this.duration <= 0) {
            this.timeRemaining = 0;
            this.progressPercentage = 0;
            return;
        }

        this.timeRemaining = this.duration;
        this.progressPercentage = 100;

        this.timerSubscription = interval(100).subscribe(() => {
            this.timeRemaining = Math.max(0, this.timeRemaining - 0.1);
            this.progressPercentage = (this.timeRemaining / this.duration) * 100;

            if (this.timeRemaining <= 0) {
                this.stopTimer();
                this.timeExpired.emit();
            }
        });
    }

    private stopTimer(): void {
        if (this.timerSubscription) {
            this.timerSubscription.unsubscribe();
            this.timerSubscription = undefined;
        }
    }

    get progressBarColor(): 'primary' | 'warn' {
        return this.timeRemaining > 10 ? 'primary' : 'warn';
    }
}
