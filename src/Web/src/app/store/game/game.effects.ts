import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { of } from 'rxjs';
import { GameService } from '../../services/game.service';
import * as GameActions from './game.actions';

@Injectable()
export class GameEffects {
  
  private actions$ = inject(Actions);
  private gameService = inject(GameService);
  private router = inject(Router);

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
      tap(() => this.router.navigate(['/game/lobby']))
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
      tap(() => this.router.navigate(['/game/lobby']))
    ),
    { dispatch: false }
  );

  leaveGame$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.leaveGame),
      switchMap(() =>
        this.gameService.leaveGame('').pipe( // Game ID would come from state
          map(() => GameActions.leaveGameSuccess()),
          catchError(() => of(GameActions.leaveGameSuccess())) // Even if API fails, clear local state
        )
      )
    )
  );

  leaveGameSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.leaveGameSuccess),
      tap(() => this.router.navigate(['/home']))
    ),
    { dispatch: false }
  );
}
