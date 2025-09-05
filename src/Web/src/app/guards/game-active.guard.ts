import { Injectable, inject } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, map, take, switchMap, of, catchError, timeout } from 'rxjs';
import { Actions, ofType } from '@ngrx/effects';

import { selectCurrentGame } from '../store/game/game.selectors';
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

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    const gameCode = route.paramMap.get('gameCode');
    
    if (!gameCode) {
      console.log('Guard: No game code in route, redirecting to home');
      this.router.navigate(['/home']);
      return of(false);
    }

    console.log('Guard: Checking access for active game:', gameCode);

    // Check if we have player data for this game code
    if (!this.gamePersistenceService.hasPlayerData(gameCode)) {
      console.log('Guard: No player data found for game code, redirecting to home');
      this.router.navigate(['/home']);
      return of(false);
    }

    // Get player data and refresh game state from server
    const playerData = this.gamePersistenceService.getPlayerData(gameCode);
    if (!playerData) {
      console.log('Guard: Failed to get player data, redirecting to home');
      this.router.navigate(['/home']);
      return of(false);
    }

    console.log('Guard: Found player data, refreshing game state from server');
    this.store.dispatch(refreshGameStateFromServer({
      gameCode: gameCode,
      playerId: playerData.playerId,
      playerName: playerData.playerName
    }));

    // Wait for server response
    return this.actions$.pipe(
      ofType(refreshGameStateFromServerSuccess, refreshGameStateFromServerFailure),
      take(1),
      timeout(10000), // 10 second timeout for server response
      switchMap(action => {
        if (action.type === refreshGameStateFromServerSuccess.type) {
          console.log('Guard: Server refresh succeeded, checking game status');
          // Check if the game is actually in active state
          return this.store.select(selectCurrentGame).pipe(
            take(1),
            map(currentGame => {
              if (currentGame && 
                  (currentGame.status === GameStatus.Active || 
                   currentGame.status === GameStatus.InProgress)) {
                console.log('Guard: Game is in active state, allowing access');
                return true;
              } else if (currentGame && currentGame.status === GameStatus.Waiting) {
                console.log('Guard: Game is still in lobby state, redirecting to lobby');
                this.router.navigate(['/game', gameCode, 'lobby']);
                return false;
              } else {
                console.log('Guard: Game is not in valid state, redirecting to home');
                this.router.navigate(['/home']);
                return false;
              }
            })
          );
        } else {
          console.log('Guard: Server refresh failed, redirecting to home');
          this.router.navigate(['/home']);
          return of(false);
        }
      }),
      catchError(() => {
        console.log('Guard: Server refresh timed out, redirecting to home');
        this.router.navigate(['/home']);
        return of(false);
      })
    );
  }
}
