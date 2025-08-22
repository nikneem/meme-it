import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, map, take, switchMap, of, filter, timeout, catchError, timer } from 'rxjs';
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
          console.log('Guard: Already in lobby, allowing access');
          return of(true);
        }
        
        // If not in lobby, check if we have persisted state to restore
        if (this.gamePersistenceService.hasValidGameState()) {
          console.log('Guard: Not in lobby but have persisted state, waiting for restoration');
          
          // Give a short delay to allow app initialization to complete first
          return timer(100).pipe(
            switchMap(() => this.store.select(selectIsInLobby).pipe(take(1))),
            switchMap(isInLobbyAfterWait => {
              if (isInLobbyAfterWait) {
                console.log('Guard: After wait, now in lobby, allowing access');
                return of(true);
              }
              
              // Still not in lobby, dispatch refresh ourselves
              const persistedState = this.gamePersistenceService.loadGameState();
              if (persistedState && persistedState.gameCode && persistedState.playerId && persistedState.playerName) {
                console.log('Guard: Dispatching server refresh as backup');
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
                      console.log('Guard: Server refresh succeeded');
                      return true;
                    } else {
                      console.log('Guard: Server refresh failed, redirecting to home');
                      this.router.navigate(['/home']);
                      return false;
                    }
                  }),
                  catchError(() => {
                    // If timeout or error, redirect to home
                    console.log('Guard: Server refresh timed out, redirecting to home');
                    this.router.navigate(['/home']);
                    return of(false);
                  })
                );
              }
              
              // No valid persisted state, redirect to home
              console.log('Guard: No valid persisted state, redirecting to home');
              this.router.navigate(['/home']);
              return of(false);
            })
          );
        }
        
        // No valid state to restore, redirect to home
        console.log('Guard: No valid state to restore, redirecting to home');
        this.router.navigate(['/home']);
        return of(false);
      })
    );
  }
}
