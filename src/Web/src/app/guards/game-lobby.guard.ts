import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, map, take, switchMap, of, filter, timeout, catchError } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';
import { selectIsInLobby } from '../store/game/game.selectors';
import { GamePersistenceService } from '../shared/services/game-persistence.service';
import { restoreGameState, restoreGameStateSuccess, restoreGameStateFailure } from '../store/game/game.actions';

@Injectable({
  providedIn: 'root'
})
export class GameLobbyGuard implements CanActivate {
  
  constructor(
    private store: Store,
    private router: Router,
    private gamePersistenceService: GamePersistenceService,
    private actions$: Actions
  ) {}

  canActivate(): Observable<boolean> {
    return this.store.select(selectIsInLobby).pipe(
      take(1),
      switchMap(isInLobby => {
        // If not in lobby, try to restore from localStorage
        if (!isInLobby && this.gamePersistenceService.hasValidGameState()) {
          this.store.dispatch(restoreGameState());
          
          // Wait for restoration success or failure
          return this.actions$.pipe(
            ofType(restoreGameStateSuccess, restoreGameStateFailure),
            take(1),
            timeout(5000), // 5 second timeout
            map(action => {
              if (action.type === restoreGameStateSuccess.type) {
                return true;
              } else {
                this.router.navigate(['/home']);
                return false;
              }
            }),
            catchError(() => {
              // If timeout or error, redirect to home
              this.router.navigate(['/home']);
              return of(false);
            })
          );
        } else if (!isInLobby) {
          this.router.navigate(['/home']);
          return of(false);
        }
        
        return of(true);
      })
    );
  }
}
