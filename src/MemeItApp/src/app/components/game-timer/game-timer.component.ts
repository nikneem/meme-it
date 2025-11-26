import { Component, Input, Output, EventEmitter, OnDestroy, OnChanges, SimpleChanges, ChangeDetectorRef, NgZone } from '@angular/core';
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
    @Input() duration: number = 0; // Duration in seconds (kept for backward compatibility)
    @Input() endTime: Date | null = null; // Phase end time (preferred)
    @Output() timeExpired = new EventEmitter<void>();

    timeRemaining = 0;
    progressPercentage = 100;
    private timerSubscription?: Subscription;

    constructor(
        private cdr: ChangeDetectorRef,
        private ngZone: NgZone
    ) { }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['duration'] || changes['endTime']) {
            this.startTimer();
        }
    }

    ngOnDestroy(): void {
        this.stopTimer();
    }

    private startTimer(): void {
        this.stopTimer();

        // Use endTime if provided, otherwise fall back to duration
        if (this.endTime) {
            const totalDuration = this.calculateTotalDuration();
            if (totalDuration <= 0) {
                this.timeRemaining = 0;
                this.progressPercentage = 0;
                return;
            }

            // Run interval inside NgZone to ensure change detection
            this.ngZone.run(() => {
                this.timerSubscription = interval(100).subscribe(() => {
                    this.updateTimerFromEndTime();
                    this.cdr.markForCheck();
                });
            });
        } else if (this.duration > 0) {
            // Fallback to duration-based countdown
            this.timeRemaining = this.duration;
            this.progressPercentage = 100;

            this.ngZone.run(() => {
                this.timerSubscription = interval(100).subscribe(() => {
                    this.timeRemaining = Math.max(0, this.timeRemaining - 0.1);
                    this.progressPercentage = (this.timeRemaining / this.duration) * 100;

                    if (this.timeRemaining <= 0) {
                        this.stopTimer();
                        this.timeExpired.emit();
                    }
                    this.cdr.markForCheck();
                });
            });
        } else {
            this.timeRemaining = 0;
            this.progressPercentage = 0;
        }
    }

    private calculateTotalDuration(): number {
        if (!this.endTime) return 0;
        const now = new Date();
        const end = new Date(this.endTime);
        return Math.max(0, (end.getTime() - now.getTime()) / 1000);
    }

    private updateTimerFromEndTime(): void {
        if (!this.endTime) return;

        const now = new Date();
        const end = new Date(this.endTime);
        const totalDuration = this.calculateTotalDuration();

        // Recalculate remaining time based on current time vs end time
        this.timeRemaining = Math.max(0, (end.getTime() - now.getTime()) / 1000);

        // Calculate progress based on original total duration
        const originalDuration = this.duration > 0 ? this.duration : totalDuration;
        if (originalDuration > 0) {
            this.progressPercentage = (this.timeRemaining / originalDuration) * 100;
        } else {
            this.progressPercentage = 0;
        }

        if (this.timeRemaining <= 0) {
            this.stopTimer();
            this.timeExpired.emit();
        }
    }

    private stopTimer(): void {
        if (this.timerSubscription) {
            this.timerSubscription.unsubscribe();
            this.timerSubscription = undefined;
        }
    }

    public resetTimer(): void {
        this.stopTimer();
        this.timeRemaining = 0;
        this.progressPercentage = 0;
        this.cdr.markForCheck();
    }

    get progressBarColor(): 'primary' | 'warn' {
        return this.timeRemaining > 10 ? 'primary' : 'warn';
    }
}
