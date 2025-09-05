import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { catchError, map, switchMap, tap, withLatestFrom, takeUntil, filter, take, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { of, Subject, EMPTY } from 'rxjs';
import { Store } from '@ngrx/store';
import { GameService } from '../../services/game.service';
import { GamePersistenceService } from '../../shared/services/game-persistence.service';
import { WebPubSubService, GameUpdateMessageTypes, GameUpdateMessage } from '../../services/web-pubsub.service';
import { WebPubSubConnectionService } from '../../services/web-pubsub-connection.service';
import { selectCurrentGame } from './game.selectors';
import { selectCurrentPlayer } from '../player/player.selectors';
import { GameStatus } from './game.models';
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
      switchMap(({ game }) => 
        // Wait for the current player to be set in the store
        this.store.select(selectCurrentPlayer).pipe(
          filter(player => !!player), // Wait until player is available
          take(1), // Take only the first emission
          tap(player => {
            console.log('Effect: Game created successfully, navigating to lobby', { 
              gameCode: game.gameCode, 
              playerId: player.id 
            });
            // Store player data for the game code
            this.gamePersistenceService.storePlayerData(game.gameCode, player.id, player.name);
            this.router.navigate(['/game', game.gameCode, 'lobby']);
          })
        )
      )
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
      switchMap(({ game }) => 
        // Wait for the current player to be set in the store
        this.store.select(selectCurrentPlayer).pipe(
          filter(player => !!player), // Wait until player is available
          take(1), // Take only the first emission
          tap(player => {
            console.log('Effect: Game joined successfully, navigating to lobby', { 
              gameCode: game.gameCode, 
              playerId: player.id 
            });
            // Store player data for the game code
            this.gamePersistenceService.storePlayerData(game.gameCode, player.id, player.name);
            this.router.navigate(['/game', game.gameCode, 'lobby']);
          })
        )
      )
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
        
        return this.gameService.leaveGame(game.gameCode, player.id).pipe(
          map(() => GameActions.leaveGameSuccess()),
          catchError(() => of(GameActions.leaveGameSuccess())) // Even if API fails, clear local state
        );
      })
    )
  );

  leaveGameSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.leaveGameSuccess),
      withLatestFrom(
        this.store.select(selectCurrentGame)
      ),
      tap(([action, game]) => {
        // Clear player data for this game code
        if (game) {
          this.gamePersistenceService.clearPlayerData(game.gameCode);
        }
        this.router.navigate(['/home']);
      })
    ),
    { dispatch: false }
  );

  // Game State Restoration is now handled by guards based on route parameters and sessionStorage

  // Refresh game state from server
  refreshGameStateFromServer$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServer),
      // Debounce to prevent rapid successive calls during multiple refreshes
      debounceTime(50), // Reduced from 100ms to 50ms for faster response
      // Only proceed if the request parameters have actually changed
      distinctUntilChanged((prev, curr) => 
        prev.gameCode === curr.gameCode && 
        prev.playerId === curr.playerId && 
        prev.playerName === curr.playerName
      ),
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
            // Game validation errors are now handled by guards
            // No need to clear persistence here as guards will redirect appropriately
            console.log('Effect: Game refresh failed:', error.message);
            return of(GameActions.refreshGameStateFromServerFailure({ 
              error: error.message || 'Failed to restore game state from server' 
            }));
          })
        )
      })
    )
  );

  // Handle successful server refresh - no persistence needed since data is in sessionStorage
  refreshGameStateFromServerSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServerSuccess),
      tap(({ game }) => {
        console.log('Effect: Server refresh successful, game restored', { 
          game: game?.gameCode, 
          gameStatus: game?.status 
        });
        // No need to save state - it's already in sessionStorage by game code
      })
    ),
    { dispatch: false }
  );

  // Auto-connect to WebPubSub when game state is restored
  refreshGameStateFromServerSuccessWebPubSub$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.refreshGameStateFromServerSuccess),
      withLatestFrom(
        this.store.select(selectCurrentPlayer)
      ),
      map(([{ game }, player]) => {
        console.log('Effect: Server refresh successful, attempting WebPubSub connection', { 
          gameCode: game.gameCode, 
          playerId: player?.id
        });
        if (game && player) {
          return GameActions.connectToWebPubSub({ 
            gameCode: game.gameCode, 
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
            // Verification failures are handled by guards, just report the error
            console.log('Game verification failed:', error.message);
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
        this.store.select(selectCurrentPlayer)
      ),
      tap(([action, game, player]) => {
        // No persistence needed - game state updates are real-time only
        console.log('Game state verification successful', { gameCode: action.game.gameCode });
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
        this.store.select(selectCurrentPlayer)
      ),
      tap(([action, game, player]) => {
        // Real-time game updates don't need persistence - sessionStorage is only updated on join/create
        console.log('Game state updated via real-time', { gameCode: game?.gameCode, action: action.type });
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
        
        return this.gameService.setPlayerReadyStatus(game.gameCode, player.id, action.isReady).pipe(
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
        this.store.select(selectCurrentPlayer)
      ),
      tap(([action, player]) => {
        // Real-time updates don't need to update sessionStorage
        console.log('Real-time game update received', { gameCode: action.game.gameCode });
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
        
        return this.gameService.startGame(game.gameCode, player.id).pipe(
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
        if (game?.gameCode) {
          this.router.navigate(['/game', game.gameCode, 'active']);
        } else {
          console.error('Cannot navigate to active game: game code is undefined', game);
        }
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
        if (game?.gameCode) {
          this.router.navigate(['/game', game.gameCode, 'active']);
        } else {
          console.error('Cannot navigate to active game: game code is undefined', game);
        }
      })
    ),
    { dispatch: false }
  );

  // Navigation effect for game status changes to Active/InProgress
  gameStatusChanged$ = createEffect(() =>
    this.actions$.pipe(
      ofType(GameActions.gameUpdated),
      withLatestFrom(this.store.select(selectCurrentGame)),
      filter(([action, currentGame]) => {
        // Ensure we have valid game data
        if (!action.game || !action.game.gameCode) {
          console.warn('Game status change ignored: missing game or game code', action.game);
          return false;
        }
        
        // Only navigate if the game status has changed to Active or InProgress
        const newStatus = action.game.status;
        const oldStatus = currentGame?.status;
        
        const isGameStarting = (newStatus === GameStatus.Active || newStatus === GameStatus.InProgress) && 
                              (oldStatus !== GameStatus.Active && oldStatus !== GameStatus.InProgress);
        
        console.log('Game status change detected:', { 
          oldStatus, 
          newStatus, 
          isGameStarting,
          gameCode: action.game.gameCode 
        });
        
        return isGameStarting;
      }),
      tap(([action, currentGame]) => {
        console.log('Game started via status change, navigating to active page:', action.game.gameCode);
        if (action.game?.gameCode) {
          this.router.navigate(['/game', action.game.gameCode, 'active']);
        } else {
          console.error('Cannot navigate to active game: game code is undefined', action.game);
        }
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
        
        return this.gameService.kickPlayer(game.gameCode, player.id, action.targetPlayerId).pipe(
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
          console.log('Ensuring WebPubSub connection for game:', game.gameCode, 'player:', player.id);
          return of(GameActions.connectToWebPubSub({ 
            gameCode: game.gameCode, 
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
            gameCode: game.gameCode, 
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
            gameCode: game.gameCode, 
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
              gameCode: message.data?.gameCode,
              gameStatus: message.data?.status,
              playerCount: message.data?.players?.length,
              players: message.data?.players?.map((p: any) => ({ id: p.id, name: p.name })),
              timestamp: new Date().toISOString()
            });
            
            if (message.data) {
              this.store.dispatch(GameActions.realTimeGameUpdated({ game: message.data }));
            } else {
              console.error('GAME_UPDATED message missing data:', message);
            }
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
            console.log('WebPubSub: Received GAME_STARTED message:', {
              messageData: message.data,
              gameCode: message.data?.gameCode,
              gameStatus: message.data?.status,
              timestamp: new Date().toISOString(),
              fullMessage: message
            });
            
            if (message.data?.gameCode) {
              this.store.dispatch(GameActions.realTimeGameStarted({ game: message.data }));
            } else {
              console.error('GAME_STARTED message missing game code:', message);
            }
            break;
            
          default:
            console.log('Unknown message type:', message.type);
        }
      });
  }
}
