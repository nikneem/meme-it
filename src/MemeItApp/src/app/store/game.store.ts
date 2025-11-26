import { computed, inject, effect } from '@angular/core';
import { patchState, signalStore, withComputed, withHooks, withMethods, withState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, tap, switchMap, catchError, of } from 'rxjs';
import { GameService } from '../services/game.service';
import { AuthService } from '../services/auth.service';
import {
    GameState,
    initialGameState,
    PersistedGameState,
    Player,
    Round,
    MemeSubmission,
    RoundPhase,
    GamePhase
} from './game-state.model';
import {
    PlayerJoinedEvent,
    PlayerStateChangedEvent,
    PlayerRemovedEvent,
    GameStartedEvent,
    RoundStartedEvent,
    CreativePhaseEndedEvent,
    ScorePhaseStartedEvent,
    RoundEndedEvent
} from '../services/realtime.service';

const STORAGE_KEY_PREFIX = 'memeit-game-state-';
const MAX_STATE_AGE_MS = 2 * 60 * 60 * 1000; // 2 hours

export const GameStore = signalStore(
    { providedIn: 'root' },
    withState<GameState>(initialGameState),

    withComputed((store) => ({
        // Current player info
        currentPlayer: computed(() =>
            store.players().find(p => p.playerId === store.currentPlayerId())
        ),

        // Check if current player is ready
        isCurrentPlayerReady: computed(() =>
            store.players().find(p => p.playerId === store.currentPlayerId())?.isReady ?? false
        ),

        // Check if all players are ready
        allPlayersReady: computed(() => {
            const players = store.players();
            return players.length >= 2 && players.every(p => p.isReady);
        }),

        // Get current round object
        currentRoundData: computed(() =>
            store.rounds().find(r => r.roundNumber === store.currentRound())
        ),

        // Check if in creative phase
        isCreativePhase: computed(() =>
            store.currentPhase() === 'creative' && store.phase() === 'in-progress'
        ),

        // Check if in scoring phase
        isScoringPhase: computed(() =>
            store.currentPhase() === 'scoring' && store.phase() === 'in-progress'
        ),

        // Check if can submit meme
        canSubmitMeme: computed(() =>
            store.currentPhase() === 'creative' &&
            store.phase() === 'in-progress' &&
            store.selectedMemeTemplateId() !== null &&
            !store.hasSubmittedMeme()
        ),

        // Check if can rate meme
        canRateMeme: computed(() =>
            store.currentPhase() === 'scoring' &&
            store.phase() === 'in-progress' &&
            store.currentMemeToRate() !== null
        ),

        // Calculate time remaining
        timeRemaining: computed(() => {
            const timer = store.timer();
            if (!timer.isActive || !timer.endTime) return 0;

            const now = new Date().getTime();
            const end = new Date(timer.endTime).getTime();
            const remaining = Math.max(0, Math.floor((end - now) / 1000));

            return remaining;
        }),

        // Get player scores sorted
        leaderboard: computed(() =>
            [...store.players()].sort((a, b) => b.score - a.score)
        ),

        // Check if game is active
        isGameActive: computed(() =>
            store.phase() === 'in-progress' && store.gameCode() !== ''
        ),

        // Get round progress
        roundProgress: computed(() => ({
            current: store.currentRound(),
            total: store.totalRounds(),
            percentage: (store.currentRound() / store.totalRounds()) * 100
        }))
    })),

    withMethods((store, gameService = inject(GameService), authService = inject(AuthService)) => ({
        /**
         * Initialize game state for a specific game code
         */
        initializeGame: rxMethod<string>(
            pipe(
                tap((gameCode) => {
                    patchState(store, {
                        isLoading: true,
                        error: null,
                        gameCode
                    });

                    // Try to restore from session storage first
                    const restored = restoreFromStorage(gameCode);
                    if (restored) {
                        console.log('Restored game state from session storage', restored);
                        patchState(store, {
                            ...restored,
                            isLoading: false // Use cached state, don't wait for server
                        });
                    }
                }),
                switchMap((gameCode) => {
                    // If we have recent cached state for an in-progress game, skip server fetch
                    // SignalR will keep the state up-to-date
                    const hasRecentCache = store.phase() === 'in-progress' && 
                                          store.lastUpdated() &&
                                          (Date.now() - new Date(store.lastUpdated()).getTime()) < 5000;

                    if (hasRecentCache) {
                        console.log('Using cached state for in-progress game, skipping server fetch');
                        return of(null);
                    }

                    return gameService.getGame(gameCode).pipe(
                        tap((game) => {
                            if (!game) return;

                            const currentPlayerId = authService.getCurrentUserId() || '';

                            // Only update certain fields from server if we don't have them
                            // Preserve round state from SignalR events
                            const updates: any = {
                                gameCode: game.gameCode,
                                createdAt: game.createdAt,
                                isAdmin: game.isAdmin,
                                currentPlayerId,
                                isLoading: false,
                                error: null
                            };

                            // Only update phase and players if we're in lobby
                            // (in-progress state is managed by SignalR events)
                            if (store.phase() === 'lobby') {
                                updates.phase = mapGamePhase(game.state);
                                updates.players = game.players.map(p => ({
                                    ...p,
                                    score: 0
                                }));
                                updates.totalRounds = game.rounds?.length || 5;
                            }

                            patchState(store, updates);

                            // Persist to session storage
                            persistToStorage(gameCode, store);
                        }),
                        catchError((error) => {
                            console.error('Failed to load game:', error);
                            patchState(store, {
                                isLoading: false,
                                error: 'Failed to load game state'
                            });
                            return of(null);
                        })
                    );
                })
            )
        ),

        /**
         * Clear game state and session storage
         */
        clearGame(gameCode: string): void {
            patchState(store, initialGameState);
            clearStorage(gameCode);
        },

        /**
         * Handle player joined event
         */
        handlePlayerJoined(event: PlayerJoinedEvent): void {
            if (event.gameCode !== store.gameCode()) return;

            const existingPlayer = store.players().find(p => p.playerId === event.playerId);
            if (!existingPlayer) {
                patchState(store, {
                    players: [
                        ...store.players(),
                        {
                            playerId: event.playerId,
                            displayName: event.displayName,
                            isReady: false,
                            score: 0
                        }
                    ],
                    lastUpdated: new Date().toISOString(),
                    version: store.version() + 1
                });

                persistToStorage(event.gameCode, store);
            }
        },

        /**
         * Handle player state changed event
         */
        handlePlayerStateChanged(event: PlayerStateChangedEvent): void {
            patchState(store, {
                players: store.players().map(p =>
                    p.playerId === event.playerId
                        ? { ...p, isReady: event.isReady }
                        : p
                ),
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(store.gameCode(), store);
        },

        /**
         * Handle player removed event
         */
        handlePlayerRemoved(event: PlayerRemovedEvent): void {
            patchState(store, {
                players: store.players().filter(p => p.playerId !== event.playerId),
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(store.gameCode(), store);
        },

        /**
         * Handle game started event
         */
        handleGameStarted(event: GameStartedEvent): void {
            if (event.gameCode !== store.gameCode()) return;

            const startTime = new Date().toISOString();
            const endTime = new Date(Date.now() + event.durationInSeconds * 1000).toISOString();

            // Create the first round
            const firstRound: Round = {
                roundNumber: event.roundNumber,
                phase: 'creative' as RoundPhase,
                startedAt: startTime,
                creativePhaseEndTime: endTime,
                submissions: [],
                playerRatings: []
            };

            patchState(store, {
                phase: 'in-progress',
                currentRound: event.roundNumber,
                currentPhase: 'creative',
                rounds: [firstRound],
                timer: {
                    duration: event.durationInSeconds,
                    endTime,
                    isActive: true
                },
                hasSubmittedMeme: false,
                selectedMemeTemplateId: null,
                playerSubmission: null,
                currentMemeToRate: null,
                memesRatedThisRound: [],
                showScoreboard: false,
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(event.gameCode, store);
        },

        /**
         * Handle round started event
         */
        handleRoundStarted(event: RoundStartedEvent): void {
            if (event.gameCode !== store.gameCode()) return;

            const startTime = new Date().toISOString();
            const endTime = new Date(Date.now() + event.durationInSeconds * 1000).toISOString();

            // Create or update round
            const existingRound = store.rounds().find(r => r.roundNumber === event.roundNumber);
            let updatedRounds: Round[];

            if (existingRound) {
                updatedRounds = store.rounds().map(r =>
                    r.roundNumber === event.roundNumber
                        ? {
                            ...r,
                            phase: 'creative' as RoundPhase,
                            startedAt: startTime,
                            creativePhaseEndTime: endTime
                        }
                        : r
                );
            } else {
                updatedRounds = [
                    ...store.rounds(),
                    {
                        roundNumber: event.roundNumber,
                        phase: 'creative' as RoundPhase,
                        startedAt: startTime,
                        creativePhaseEndTime: endTime,
                        submissions: [],
                        playerRatings: []
                    }
                ];
            }

            patchState(store, {
                currentRound: event.roundNumber,
                currentPhase: 'creative',
                rounds: updatedRounds,
                timer: {
                    duration: event.durationInSeconds,
                    endTime,
                    isActive: true
                },
                hasSubmittedMeme: false,
                selectedMemeTemplateId: null,
                playerSubmission: null,
                currentMemeToRate: null,
                memesRatedThisRound: [],
                showScoreboard: false,
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(event.gameCode, store);
        },

        /**
         * Handle creative phase ended event
         */
        handleCreativePhaseEnded(event: CreativePhaseEndedEvent): void {
            if (event.gameCode !== store.gameCode()) return;

            patchState(store, {
                currentPhase: 'scoring',
                timer: {
                    duration: 0,
                    endTime: null,
                    isActive: false
                },
                rounds: store.rounds().map(r =>
                    r.roundNumber === event.roundNumber
                        ? { ...r, phase: 'scoring' as RoundPhase }
                        : r
                ),
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(event.gameCode, store);
        },

        /**
         * Handle score phase started event
         */
        handleScorePhaseStarted(event: ScorePhaseStartedEvent): void {
            if (event.gameCode !== store.gameCode()) return;

            // Store basic meme submission data
            // Note: This will be enriched by the component with template details
            const memeSubmission: MemeSubmission = {
                memeId: event.memeId,
                playerId: event.playerId,
                playerName: store.players().find(p => p.playerId === event.playerId)?.displayName || 'Player',
                memeTemplateId: event.memeTemplateId,
                textEntries: event.textEntries,
                submittedAt: new Date().toISOString()
            };

            const endTime = new Date(Date.now() + event.ratingDurationSeconds * 1000).toISOString();

            patchState(store, {
                currentPhase: 'scoring',
                currentMemeToRate: memeSubmission,
                timer: {
                    duration: event.ratingDurationSeconds,
                    endTime,
                    isActive: true
                },
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(event.gameCode, store);
        },

        /**
         * Handle round ended event
         */
        handleRoundEnded(event: RoundEndedEvent): void {
            if (event.gameCode !== store.gameCode()) return;

            // Update player scores from scoreboard
            const updatedPlayers = store.players().map(player => {
                const scoreEntry = event.scoreboard.find(s => s.playerId === player.playerId);
                return scoreEntry
                    ? { ...player, score: scoreEntry.totalScore }
                    : player;
            });

            patchState(store, {
                currentPhase: 'ended',
                players: updatedPlayers,
                showScoreboard: true,
                finalScores: event.scoreboard,
                totalRounds: event.totalRounds,
                timer: {
                    duration: 0,
                    endTime: null,
                    isActive: false
                },
                rounds: store.rounds().map(r =>
                    r.roundNumber === event.roundNumber
                        ? {
                            ...r,
                            phase: 'ended' as RoundPhase,
                            endedAt: new Date().toISOString()
                        }
                        : r
                ),
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            // Check if game is completed
            if (event.roundNumber >= event.totalRounds) {
                patchState(store, {
                    phase: 'completed'
                });
            }

            persistToStorage(event.gameCode, store);
        },

        /**
         * Update selected meme template
         */
        selectMemeTemplate(templateId: string): void {
            patchState(store, {
                selectedMemeTemplateId: templateId,
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(store.gameCode(), store);
        },

        /**
         * Mark meme as submitted
         */
        markMemeSubmitted(submission: MemeSubmission): void {
            patchState(store, {
                hasSubmittedMeme: true,
                playerSubmission: submission,
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(store.gameCode(), store);
        },

        /**
         * Record meme rating
         */
        recordMemeRating(memeId: string, rating: number): void {
            patchState(store, {
                memesRatedThisRound: [...store.memesRatedThisRound(), memeId],
                currentMemeToRate: null,
                lastUpdated: new Date().toISOString(),
                version: store.version() + 1
            });

            persistToStorage(store.gameCode(), store);
        },

        /**
         * Manually persist current state to storage
         */
        persist(): void {
            persistToStorage(store.gameCode(), store);
        }
    })),

    withHooks({
        onInit(store) {
            // Auto-persist on any state change
            effect(() => {
                const gameCode = store.gameCode();
                const version = store.version();

                if (gameCode && version > 1) {
                    persistToStorage(gameCode, store);
                }
            });
        },
        onDestroy() {
            console.log('Game store destroyed');
        }
    })
);

// Helper functions

function mapGamePhase(state: string): GamePhase {
    switch (state.toLowerCase()) {
        case 'waitingforplayers':
        case 'lobby':
            return 'lobby';
        case 'inprogress':
        case 'scoring':
            return 'in-progress';
        case 'completed':
            return 'completed';
        default:
            return 'lobby';
    }
}

function updateRoundFromServerState(store: any, game: any): void {
    const currentRound = game.currentRoundInfo;

    if (!currentRound) return;

    const roundPhase: RoundPhase = currentRound.phase.toLowerCase() as RoundPhase;
    const now = new Date().toISOString();

    let timerDuration = 0;
    let timerEndTime: string | null = null;
    let isTimerActive = false;

    if (roundPhase === 'creative' && currentRound.creativePhaseEndTime) {
        const endTime = new Date(currentRound.creativePhaseEndTime);
        const remaining = Math.max(0, Math.floor((endTime.getTime() - Date.now()) / 1000));
        timerDuration = remaining;
        timerEndTime = currentRound.creativePhaseEndTime;
        isTimerActive = remaining > 0;
    }

    patchState(store, {
        currentRound: currentRound.roundNumber,
        currentPhase: roundPhase,
        timer: {
            duration: timerDuration,
            endTime: timerEndTime,
            isActive: isTimerActive
        },
        hasSubmittedMeme: !!game.playerSubmission,
        playerSubmission: game.playerSubmission || null
    });
}

function persistToStorage(gameCode: string, store: any): void {
    if (!gameCode) return;

    try {
        const state = {
            gameCode: store.gameCode(),
            phase: store.phase(),
            createdAt: store.createdAt(),
            isAdmin: store.isAdmin(),
            players: store.players(),
            currentPlayerId: store.currentPlayerId(),
            rounds: store.rounds(),
            currentRound: store.currentRound(),
            totalRounds: store.totalRounds(),
            currentPhase: store.currentPhase(),
            timer: store.timer(),
            selectedMemeTemplateId: store.selectedMemeTemplateId(),
            hasSubmittedMeme: store.hasSubmittedMeme(),
            playerSubmission: store.playerSubmission(),
            currentMemeToRate: store.currentMemeToRate(),
            memesRatedThisRound: store.memesRatedThisRound(),
            showScoreboard: store.showScoreboard(),
            finalScores: store.finalScores(),
            isLoading: store.isLoading(),
            error: store.error(),
            lastUpdated: store.lastUpdated(),
            version: store.version(),
            persistedAt: new Date().toISOString()
        } as PersistedGameState;

        sessionStorage.setItem(
            `${STORAGE_KEY_PREFIX}${gameCode}`,
            JSON.stringify(state)
        );
    } catch (error) {
        console.error('Failed to persist game state:', error);
    }
}

function restoreFromStorage(gameCode: string): GameState | null {
    try {
        const stored = sessionStorage.getItem(`${STORAGE_KEY_PREFIX}${gameCode}`);
        if (!stored) return null;

        const state = JSON.parse(stored) as PersistedGameState;

        // Remove persisted metadata
        const { persistedAt, ...gameState } = state;

        return gameState;
    } catch (error) {
        console.error('Failed to restore game state:', error);
        return null;
    }
}

function clearStorage(gameCode: string): void {
    try {
        sessionStorage.removeItem(`${STORAGE_KEY_PREFIX}${gameCode}`);
    } catch (error) {
        console.error('Failed to clear game state:', error);
    }
}

function isStateValid(state: PersistedGameState): boolean {
    // Check if state is not too old
    const persistedAt = new Date(state.persistedAt).getTime();
    const now = Date.now();

    if (now - persistedAt > MAX_STATE_AGE_MS) {
        console.log('Persisted state is too old, will fetch fresh from server');
        return false;
    }

    // Check if required fields are present
    if (!state.gameCode || !state.currentPlayerId) {
        return false;
    }

    return true;
}
