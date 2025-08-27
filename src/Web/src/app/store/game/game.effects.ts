import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { catchError, map, switchMap, tap, withLatestFrom, takeUntil, filter, take } from 'rxjs/operators';
import { of, Subject, EMPTY } from 'rxjs';
import { Store } from '@ngrx/store';
import { GameService } from '../../services/game.service';
import { GamePersistenceService } from '../../shared/services/game-persistence.service';
import { WebPubSubService, GameUpdateMessageTypes, GameUpdateMessage } from '../../services/web-pubsub.service';
import { WebPubSubConnectionService } from '../../services/web-pubsub-connection.service';
import { selectCurrentGame, selectIsInLobby } from './game.selectors';
import { selectCurrentPlayer } from '../player/player.selectors';
import * as GameActions from './game.actions';
import * as PlayerActions from '../player/player.actions';

@Injectable()
export class GameEffects {
  
  private actions$ = inject(Actions);
  private gameService = inject(GameService);
  private router = inject(Router);
  private store = inject(Store);
  private gamePersistenceService = inject(GamePersistenceService);
  private webPubSubService = inject(WebPubSubService);
  private webPubSubConnectionService = inject(WebPubSubConnectionService);

  private destroy$ = new Subject<void>();

  createGame$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.createGame),
      switchMap(({ request }) =>
        this.gameService.createGame(request).pipe(
          switchMap(({ game, player }) => [
            GameActions.createGameSuccess({ game }),
            PlayerActions.setCurrentPlayer({ player })
          ]),
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
          switchMap(({ game, player }) => [
            GameActions.joinGameSuccess({ game }),
            PlayerActions.setCurrentPlayer({ player })
          ]),
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
      switchMap(({ gameCode, playerId, playerName }) => {
        console.log('Effect: Starting server refresh for', { gameCode, playerId, playerName });
        return this.gameService.validateGameAndPlayer(gameCode, playerId, playerName).pipe(
          switchMap(response => {
            console.log('Effect: Server validation response', response);
            if (response.isValid) {
              return [
                GameActions.refreshGameStateFromServerSuccess({
                  game: response.game
                }),
                PlayerActions.setCurrentPlayer({ player: response.player })
              ];
            } else {
              throw new Error('Player is no longer in the game or game does not exist');
            }
          }),
          catchError(error => {
            console.error('Effect: Server refresh failed', error);
            // Clear invalid persisted state
            this.gamePersistenceService.clearGameState();
            this.router.navigate(['/home']);
            return of(GameActions.refreshGameStateFromServerFailure({ 
              error: error.message || 'Failed to restore game state from server' 
            }));
          })
        )
      })
    )
  );

  // Handle successful server refresh
  refreshGameStateFromServerSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServerSuccess),
      withLatestFrom(this.store.select(selectCurrentPlayer)),
      tap(([{ game }, player]) => {
        console.log('Effect: Server refresh successful, updating persistence', { game, player });
        // Update persisted state with fresh data from server
        this.gamePersistenceService.saveGameState(game, player, true);
      })
    ),
    { dispatch: false }
  );

  // Auto-connect to WebPubSub when game state is restored
  refreshGameStateFromServerSuccessWebPubSub$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServerSuccess),
      withLatestFrom(this.store.select(selectCurrentPlayer)),
      map(([{ game }, player]) => {
        console.log('Effect: Server refresh successful, attempting WebPubSub connection', { gameCode: game.code, playerId: player?.id });
        if (game && player) {
          return GameActions.connectToWebPubSub({ 
            gameCode: game.code, 
            playerId: player.id 
          });
        }
        return { type: 'NO_ACTION' };
      }),
      filter(action => action.type !== 'NO_ACTION')
    )
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
          switchMap(({ game: updatedGame, player: updatedPlayer }) => [
            GameActions.setPlayerReadyStatusSuccess({ game: updatedGame }),
            PlayerActions.updatePlayerReadyStatus({ isReady: updatedPlayer.isReady })
          ]),
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
        this.store.select(selectCurrentPlayer),
        this.store.select(selectIsInLobby)
      ),
      tap(([action, player, isInLobby]) => {
        // Update persisted state with new data
        this.gamePersistenceService.saveGameState(action.game, player, isInLobby);
      })
    ),
    { dispatch: false }
  );

  // Start Game Effects
  startGame$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.startGame),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer)
      ),
      switchMap(([action, game, player]) => {
        if (!game || !player) {
          return of(GameActions.startGameFailure({ error: 'Game or player not found' }));
        }
        
        return this.gameService.startGame(game.code, player.id).pipe(
          map((updatedGame) => 
            GameActions.startGameSuccess({ game: updatedGame })
          ),
          catchError((error) =>
            of(GameActions.startGameFailure({ error: error.message || 'Failed to start game' }))
          )
        );
      })
    )
  );

  startGameSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.startGameSuccess),
      tap(({ game }) => {
        // Navigate to active game page when game starts
        console.log('Game started successfully:', game);
        this.router.navigate(['/game/active']);
      })
    ),
    { dispatch: false }
  );

  // Navigation effect for real-time game start events
  realTimeGameStarted$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.realTimeGameStarted),
      tap(({ game }) => {
        console.log('Real-time game started event received:', game);
        this.router.navigate(['/game/active']);
      })
    ),
    { dispatch: false }
  );

  // Kick Player Effects
  kickPlayer$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.kickPlayer),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer)
      ),
      switchMap(([action, game, player]) => {
        if (!game || !player) {
          return of(GameActions.kickPlayerFailure({ error: 'Game or player not found' }));
        }
        
        return this.gameService.kickPlayer(game.code, player.id, action.targetPlayerId).pipe(
          map((updatedGame) => 
            GameActions.kickPlayerSuccess({ game: updatedGame })
          ),
          catchError((error) =>
            of(GameActions.kickPlayerFailure({ error: error.message || 'Failed to kick player' }))
          )
        );
      })
    )
  );

  kickPlayerSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.kickPlayerSuccess),
      tap(({ game }) => {
        console.log('Player kicked successfully, game updated:', game);
      })
    ),
    { dispatch: false }
  );

  // WebPubSub Connection Effects
  connectToWebPubSub$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.connectToWebPubSub),
      switchMap(({ gameCode, playerId }) => {
        console.log('Effect: Starting WebPubSub connection', { gameCode, playerId });
        return this.webPubSubConnectionService.getConnectionUrl(gameCode, playerId).pipe(
          switchMap((connectionResponse) => {
            console.log('Effect: WebPubSub connection URL received', connectionResponse);
            if (connectionResponse.isSuccess) {
              return this.webPubSubService.connect(connectionResponse.connectionUrl, gameCode).then(() => {
                console.log('Effect: WebPubSub connected successfully');
                // Start listening to messages after successful connection
                this.startListeningToMessages();
                return GameActions.connectToWebPubSubSuccess({ 
                  connectionUrl: connectionResponse.connectionUrl, 
                  gameCode 
                });
              }).catch((error) => {
                console.error('Effect: WebPubSub connection failed', error);
                return GameActions.connectToWebPubSubFailure({ 
                  error: error.message || 'Failed to connect to WebPubSub' 
                });
              });
            } else {
              console.error('Effect: Failed to get WebPubSub connection URL', connectionResponse.errorMessage);
              return of(GameActions.connectToWebPubSubFailure({ 
                error: connectionResponse.errorMessage || 'Failed to get connection URL' 
              }));
            }
          }),
          catchError((error) => {
            console.error('Effect: WebPubSub connection error', error);
            return of(GameActions.connectToWebPubSubFailure({ 
              error: error.message || 'Failed to connect to WebPubSub' 
            }));
          })
        )
      })
    )
  );

  connectToWebPubSubSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.connectToWebPubSubSuccess),
      tap(({ gameCode }) => {
        console.log('Successfully connected to WebPubSub for game:', gameCode);
      })
    ),
    { dispatch: false }
  );

  // Ensure WebPubSub connection when manually triggered
  ensureWebPubSubConnection$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.ensureWebPubSubConnection),
      withLatestFrom(
        this.store.select(selectCurrentGame),
        this.store.select(selectCurrentPlayer)
      ),
      switchMap(([action, game, player]) => {
        if (game && player && !this.webPubSubService.isConnected()) {
          console.log('Ensuring WebPubSub connection for game:', game.code, 'player:', player.id);
          return of(GameActions.connectToWebPubSub({ 
            gameCode: game.code, 
            playerId: player.id 
          }));
        }
        return EMPTY;
      })
    )
  );

  // Auto-connect to WebPubSub when joining a game successfully
  joinGameSuccessWebPubSub$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.joinGameSuccess),
      withLatestFrom(
        this.store.select(selectCurrentPlayer)
      ),
      map(([{ game }, player]) => {
        if (game && player) {
          return GameActions.connectToWebPubSub({ 
            gameCode: game.code, 
            playerId: player.id 
          });
        }
        return { type: 'NO_ACTION' };
      }),
      filter(action => action.type !== 'NO_ACTION')
    )
  );

  // Auto-connect to WebPubSub when creating a game successfully
  createGameSuccessWebPubSub$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.createGameSuccess),
      withLatestFrom(
        this.store.select(selectCurrentPlayer)
      ),
      map(([{ game }, player]) => {
        if (game && player) {
          return GameActions.connectToWebPubSub({ 
            gameCode: game.code, 
            playerId: player.id 
          });
        }
        return { type: 'NO_ACTION' };
      }),
      filter(action => action.type !== 'NO_ACTION')
    )
  );

  // Disconnect from WebPubSub when leaving game
  leaveGameWebPubSub$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.leaveGame),
      tap(() => {
        this.webPubSubService.disconnect();
        this.destroy$.next();
      })
    ),
    { dispatch: false }
  );

  // Real-time message effects
  realTimeGameUpdated$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.realTimeGameUpdated),
      map(({ game }) => GameActions.gameUpdated({ game }))
    )
  );

  realTimePlayerJoined$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.realTimePlayerJoined),
      map(({ player }) => GameActions.playerJoined({ player }))
    )
  );

  realTimePlayerLeft$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.realTimePlayerLeft),
      map(({ playerId }) => GameActions.playerLeft({ playerId }))
    )
  );

  private startListeningToMessages(): void {
    this.webPubSubService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe((message: GameUpdateMessage) => {
        console.log('Received real-time message:', message);
        
        switch (message.type) {
          case GameUpdateMessageTypes.GAME_UPDATED:
            console.log('WebPubSub: Received GAME_UPDATED message:', {
              messageData: message.data,
              playerCount: message.data?.players?.length,
              players: message.data?.players?.map((p: any) => ({ id: p.id, name: p.name })),
              timestamp: new Date().toISOString()
            });
            this.store.dispatch(GameActions.realTimeGameUpdated({ game: message.data }));
            break;
            
          case GameUpdateMessageTypes.PLAYER_JOINED:
            console.log('WebPubSub: Received PLAYER_JOINED message:', {
              messageData: message.data,
              playerId: message.data?.id,
              playerName: message.data?.name,
              timestamp: new Date().toISOString(),
              fullMessage: message
            });
            
            // Get current game state for debugging
            this.store.select(selectCurrentGame).pipe(
              take(1)
            ).subscribe(currentGame => {
              console.log('Current game state when PLAYER_JOINED received:', {
                currentGame: currentGame,
                existingPlayers: currentGame?.players?.map((p: any) => ({ id: p.id, name: p.name })),
                newPlayerAlreadyExists: currentGame?.players?.some((p: any) => p.id === message.data?.id)
              });
            });
            
            this.store.dispatch(GameActions.realTimePlayerJoined({ player: message.data }));
            break;
            
          case GameUpdateMessageTypes.PLAYER_LEFT:
            this.store.dispatch(GameActions.realTimePlayerLeft({ playerId: message.data.playerId || message.data.id }));
            break;
            
          case GameUpdateMessageTypes.PLAYER_READY_STATUS_CHANGED:
            this.store.dispatch(GameActions.realTimePlayerReadyStatusChanged({ 
              playerId: message.data.playerId || message.data.id, 
              isReady: message.data.isReady 
            }));
            break;
            
          case GameUpdateMessageTypes.PLAYER_KICKED:
            this.store.dispatch(GameActions.realTimePlayerKicked({ 
              playerId: message.data.playerId || message.data.id 
            }));
            break;
            
          case GameUpdateMessageTypes.GAME_STARTED:
            this.store.dispatch(GameActions.realTimeGameStarted({ game: message.data }));
            break;
            
          default:
            console.log('Unknown message type:', message.type);
        }
      });
  }
}
