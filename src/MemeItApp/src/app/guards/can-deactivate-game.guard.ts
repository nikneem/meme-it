import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { LeaveGameDialogComponent } from '../components/leave-game-dialog/leave-game-dialog.component';

export interface CanComponentDeactivate {
    canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}

export const canDeactivateGameGuard: CanDeactivateFn<CanComponentDeactivate> = (
    component
): Observable<boolean> | boolean => {
    // If component implements canDeactivate, use it
    if (component && component.canDeactivate) {
        const result = component.canDeactivate();

        // If already returning true, allow navigation
        if (result === true) {
            return true;
        }

        // If returning false or Observable, show confirmation dialog
        const dialog = inject(MatDialog);
        const dialogRef = dialog.open(LeaveGameDialogComponent, {
            width: '500px',
            maxWidth: '90vw',
            disableClose: true,
            panelClass: 'leave-game-dialog',
            backdropClass: 'leave-game-backdrop'
        });

        return dialogRef.afterClosed().pipe(
            map(result => result === true)
        );
    }

    // Default: allow navigation
    return true;
};
