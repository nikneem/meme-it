import { Injectable, inject } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, map, take, switchMap, of, timer, catchError } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';

import { selectCurrentGame, selectIsInLobby } from '../store/game/game.selectors';
import { GamePersistenceService } from '../shared/services/game-persistence.service';
import { 
  refreshGameStateFromServer,
  refreshGameStateFromServerSuccess, 
  refreshGameStateFromServerFailure
} from '../store/game/game.actions';
import { GameStatus } from '../store/game/game.models';

@Injectable({
  providedIn: 'root'
})
export class GameActiveGuard implements CanActivate {
  private readonly store = inject(Store);
  private readonly router = inject(Router);
  private readonly gamePersistenceService = inject(GamePersistenceService);
  private readonly actions$ = inject(Actions);

  canActivate(): Observable<boolean> {
    return this.store.select(selectCurrentGame).pipe(
      take(1),
      switchMap(currentGame => {
        // Check if there's an active game
        if (currentGame && 
            (currentGame.status === GameStatus.Active || 
             currentGame.status === GameStatus.InProgress)) {
          console.log('Guard: Already have active game, allowing access');
          return of(true);
        }

        // If no active game, try to restore from persistence
        if (this.gamePersistenceService.hasValidGameState()) {
          console.log('Guard: No active game but have persisted state, attempting restoration');
          
          // Give a short delay to allow app initialization to complete first
          return timer(100).pipe(
            switchMap(() => this.store.select(selectCurrentGame).pipe(take(1))),
            switchMap(gameAfterWait => {
              if (gameAfterWait && 
                  (gameAfterWait.status === GameStatus.Active || 
                   gameAfterWait.status === GameStatus.InProgress)) {
                console.log('Guard: After wait, now have active game, allowing access');
                return of(true);
              }
              
              // Still no active game, dispatch refresh ourselves
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
                  map(action => {
                    if (action.type === refreshGameStateFromServerSuccess.type) {
                      const game = (action as any).game;
                      if (game && 
                          (game.status === GameStatus.Active || 
                           game.status === GameStatus.InProgress)) {
                        console.log('Guard: Server refresh succeeded with active game');
                        return true;
                      } else {
                        console.log('Guard: Server refresh succeeded but game is not active, redirecting to lobby');
                        this.router.navigate(['/game/lobby']);
                        return false;
                      }
                    } else {
                      console.log('Guard: Server refresh failed, redirecting to home');
                      this.router.navigate(['/home']);
                      return false;
                    }
                  }),
                  catchError(() => {
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
