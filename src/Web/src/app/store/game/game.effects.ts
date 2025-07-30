import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { catchError, map, switchMap, tap, withLatestFrom } from 'rxjs/operators';
import { of } from 'rxjs';
import { Store } from '@ngrx/store';
import { GameService } from '../../services/game.service';
import { GamePersistenceService } from '../../shared/services/game-persistence.service';
import { selectCurrentGame, selectCurrentPlayer, selectIsInLobby } from './game.selectors';
import * as GameActions from './game.actions';

@Injectable()
export class GameEffects {
  
  private actions$ = inject(Actions);
  private gameService = inject(GameService);
  private router = inject(Router);
  private store = inject(Store);
  private gamePersistenceService = inject(GamePersistenceService);

  createGame$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.createGame),
      switchMap(({ request }) =>
        this.gameService.createGame(request).pipe(
          map(({ game, player }) => GameActions.createGameSuccess({ game, player })),
          catchError((error) => 
            of(GameActions.createGameFailure({ error: error.message || 'Failed to create game' }))
          )
        )
      )
    )
  );

  createGameSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.createGameSuccess),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer),
        this.store.select(selectIsInLobby)
      ),
      tap(([action, game, player, isInLobby]) => {
        // Save game state to localStorage
        this.gamePersistenceService.saveGameState(game, player, isInLobby);
        this.router.navigate(['/game/lobby']);
      })
    ), 
    { dispatch: false }
  );

  joinGame$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.joinGame),
      switchMap(({ request }) =>
        this.gameService.joinGame(request).pipe(
          map(({ game, player }) => GameActions.joinGameSuccess({ game, player })),
          catchError((error) =>
            of(GameActions.joinGameFailure({ error: error.message || 'Failed to join game' }))
          )
        )
      )
    )
  );

  joinGameSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.joinGameSuccess),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer),
        this.store.select(selectIsInLobby)
      ),
      tap(([action, game, player, isInLobby]) => {
        // Save game state to localStorage
        this.gamePersistenceService.saveGameState(game, player, isInLobby);
        this.router.navigate(['/game/lobby']);
      })
    ),
    { dispatch: false }
  );

  leaveGame$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.leaveGame),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer)
      ),
      switchMap(([action, game, player]) => {
        if (!game || !player) {
          return of(GameActions.leaveGameSuccess());
        }
        
        return this.gameService.leaveGame(game.code, player.id).pipe(
          map(() => GameActions.leaveGameSuccess()),
          catchError(() => of(GameActions.leaveGameSuccess())) // Even if API fails, clear local state
        );
      })
    )
  );

  leaveGameSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.leaveGameSuccess),
      tap(() => {
        // Clear game state from localStorage
        this.gamePersistenceService.clearGameState();
        this.router.navigate(['/home']);
      })
    ),
    { dispatch: false }
  );

  // Game State Restoration - Now uses server as source of truth
  restoreGameState$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.restoreGameState),
      switchMap(() => {
        const persistedState = this.gamePersistenceService.loadGameState();
        
        if (persistedState && persistedState.gameCode && persistedState.playerId && persistedState.playerName) {
          // Dispatch server refresh action with persisted data
          return of(GameActions.refreshGameStateFromServer({
            gameCode: persistedState.gameCode,
            playerId: persistedState.playerId,
            playerName: persistedState.playerName
          }));
        } else {
          return of(GameActions.restoreGameStateFailure());
        }
      })
    )
  );

  // Refresh game state from server
  refreshGameStateFromServer$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServer),
      switchMap(({ gameCode, playerId, playerName }) =>
        this.gameService.validateGameAndPlayer(gameCode, playerId, playerName).pipe(
          map(response => {
            if (response.isValid) {
              return GameActions.refreshGameStateFromServerSuccess({
                game: response.game,
                player: response.player
              });
            } else {
              throw new Error('Player is no longer in the game or game does not exist');
            }
          }),
          catchError(error => {
            // Clear invalid persisted state
            this.gamePersistenceService.clearGameState();
            this.router.navigate(['/home']);
            return of(GameActions.refreshGameStateFromServerFailure({ 
              error: error.message || 'Failed to restore game state from server' 
            }));
          })
        )
      )
    )
  );

  // Handle successful server refresh
  refreshGameStateFromServerSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServerSuccess),
      tap(({ game, player }) => {
        // Update persisted state with fresh data from server
        this.gamePersistenceService.saveGameState(game, player, true);
      })
    ),
    { dispatch: false }
  );

  // Game State Verification (kept for backward compatibility or manual verification)
  verifyGameState$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.verifyGameState),
      switchMap(({ gameId, playerId }) =>
        this.gameService.getGame(gameId).pipe(
          map(game => {
            // Check if the player is still in the game
            const playerStillInGame = game.players.some(p => p.id === playerId);
            if (!playerStillInGame) {
              throw new Error('Player is no longer in the game');
            }
            return GameActions.verifyGameStateSuccess({ game });
          }),
          catchError(error => {
            // If verification fails, clear persisted state and redirect
            this.gamePersistenceService.clearGameState();
            this.router.navigate(['/home']);
            return of(GameActions.verifyGameStateFailure({ 
              error: error.message || 'Game no longer exists or player removed' 
            }));
          })
        )
      )
    )
  );

  verifyGameStateSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.verifyGameStateSuccess),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer),
        this.store.select(selectIsInLobby)
      ),
      tap(([action, game, player, isInLobby]) => {
        // Update persisted state with verified data
        this.gamePersistenceService.saveGameState(action.game, player, isInLobby);
      })
    ),
    { dispatch: false }
  );

  // Persist state on any game updates
  persistGameState$ = createEffect(() =>
    this.actions$.pipe(
      ofType(
        GameActions.gameUpdated,
        GameActions.playerJoined,
        GameActions.playerLeft
      ),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer),
        this.store.select(selectIsInLobby)
      ),
      tap(([action, game, player, isInLobby]) => {
        if (game && player && isInLobby) {
          this.gamePersistenceService.saveGameState(game, player, isInLobby);
        }
      })
    ),
    { dispatch: false }
  );

  // Player Ready Status Effects
  setPlayerReadyStatus$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.setPlayerReadyStatus),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer)
      ),
      switchMap(([action, game, player]) => {
        if (!game || !player) {
          return of(GameActions.setPlayerReadyStatusFailure({ error: 'Game or player not found' }));
        }
        
        return this.gameService.setPlayerReadyStatus(game.code, player.id, action.isReady).pipe(
          map(({ game: updatedGame, player: updatedPlayer }) => 
            GameActions.setPlayerReadyStatusSuccess({ game: updatedGame, player: updatedPlayer })
          ),
          catchError((error) =>
            of(GameActions.setPlayerReadyStatusFailure({ error: error.message || 'Failed to set ready status' }))
          )
        );
      })
    )
  );

  setPlayerReadyStatusSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.setPlayerReadyStatusSuccess),
      withLatestFrom(
        this.store.select(selectIsInLobby)
      ),
      tap(([action, isInLobby]) => {
        // Update persisted state with new data
        this.gamePersistenceService.saveGameState(action.game, action.player, isInLobby);
      })
    ),
    { dispatch: false }
  );
}
