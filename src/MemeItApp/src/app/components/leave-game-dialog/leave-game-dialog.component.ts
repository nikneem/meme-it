import { Component } from '@angular/core';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'memeit-leave-game-dialog',
    imports: [
        MatDialogModule,
        MatButtonModule,
        MatIconModule
    ],
    templateUrl: './leave-game-dialog.component.html',
    styleUrl: './leave-game-dialog.component.scss',
})
export class LeaveGameDialogComponent {
    constructor(private dialogRef: MatDialogRef<LeaveGameDialogComponent>) { }

    onCancel(): void {
        this.dialogRef.close(false);
    }

    onConfirm(): void {
        this.dialogRef.close(true);
    }
}
