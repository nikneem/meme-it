import { Component, Inject } from '@angular/core';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface NewGameInvitationData {
    initiatedByPlayerName: string;
    newGameCode: string;
}

@Component({
    selector: 'memeit-new-game-invitation-dialog',
    imports: [
        MatDialogModule,
        MatButtonModule,
        MatIconModule
    ],
    templateUrl: './new-game-invitation-dialog.component.html',
    styleUrl: './new-game-invitation-dialog.component.scss',
})
export class NewGameInvitationDialogComponent {
    constructor(
        private dialogRef: MatDialogRef<NewGameInvitationDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: NewGameInvitationData
    ) { }

    onDecline(): void {
        this.dialogRef.close(false);
    }

    onAccept(): void {
        this.dialogRef.close(true);
    }
}
