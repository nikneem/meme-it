import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, map, take, switchMap, of, filter, timeout, catchError } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';
import { selectIsInLobby } from '../store/game/game.selectors';
import { GamePersistenceService } from '../shared/services/game-persistence.service';
import { 
  refreshGameStateFromServer,
  refreshGameStateFromServerSuccess, 
  refreshGameStateFromServerFailure
} from '../store/game/game.actions';

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
        // If already in lobby, allow access
        if (isInLobby) {
          return of(true);
        }
        
        // If not in lobby, try to restore from server using persisted data
        if (this.gamePersistenceService.hasValidGameState()) {
          const persistedState = this.gamePersistenceService.loadGameState();
          
          if (persistedState && persistedState.gameCode && persistedState.playerId && persistedState.playerName) {
            // Dispatch direct server refresh with persisted data
            this.store.dispatch(refreshGameStateFromServer({
              gameCode: persistedState.gameCode,
              playerId: persistedState.playerId,
              playerName: persistedState.playerName
            }));
            
            // Wait for server restoration success or failure
            return this.actions$.pipe(
              ofType(refreshGameStateFromServerSuccess, refreshGameStateFromServerFailure),
              take(1),
              timeout(10000), // 10 second timeout for server response
              map(action => {
                if (action.type === refreshGameStateFromServerSuccess.type) {
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
          }
        }
        
        // No valid state to restore, redirect to home
        this.router.navigate(['/home']);
        return of(false);
      })
    );
  }
}
