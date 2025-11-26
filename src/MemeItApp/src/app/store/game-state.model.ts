/**
 * Comprehensive game state models for the Signals Store
 * Contains all game-related state including rounds, submissions, scores, and players
 */

export type GamePhase = 'lobby' | 'in-progress' | 'completed';
export type RoundPhase = 'creative' | 'scoring' | 'ended';

export interface Player {
    playerId: string;
    displayName: string;
    isReady: boolean;
    score: number;
}

export interface TextEntry {
    textFieldId: string;
    value: string;
}

export interface MemeSubmission {
    memeId: string;
    playerId: string;
    playerName: string;
    memeTemplateId: string;
    textEntries: TextEntry[];
    submittedAt: string;
    averageRating?: number;
    totalScore?: number;
}

export interface PlayerRating {
    memeId: string;
    rating: number;
    ratedAt: string;
}

export interface Round {
    roundNumber: number;
    phase: RoundPhase;
    startedAt: string;
    creativePhaseEndTime?: string;
    scoringPhaseEndTime?: string;
    endedAt?: string;
    submissions: MemeSubmission[];
    currentMemeToRate?: MemeSubmission;
    playerRatings: PlayerRating[];
}

export interface GameTimer {
    duration: number;
    endTime: string | null;
    isActive: boolean;
}

export interface GameState {
    // Game metadata
    gameCode: string;
    phase: GamePhase;
    createdAt: string;
    isAdmin: boolean;

    // Players
    players: Player[];
    currentPlayerId: string;

    // Rounds
    rounds: Round[];
    currentRound: number;
    totalRounds: number;

    // Current round state
    currentPhase: RoundPhase;
    timer: GameTimer;

    // Player's current round state
    selectedMemeTemplateId: string | null;
    hasSubmittedMeme: boolean;
    playerSubmission: MemeSubmission | null;

    // Scoring state
    currentMemeToRate: MemeSubmission | null;
    memesRatedThisRound: string[];

    // Scoreboard
    showScoreboard: boolean;
    finalScores: Array<{
        playerId: string;
        playerName: string;
        totalScore: number;
    }>;

    // Loading states
    isLoading: boolean;
    error: string | null;

    // Metadata
    lastUpdated: string;
    version: number;
}

export const initialGameState: GameState = {
    gameCode: '',
    phase: 'lobby',
    createdAt: '',
    isAdmin: false,
    players: [],
    currentPlayerId: '',
    rounds: [],
    currentRound: 0,
    totalRounds: 5,
    currentPhase: 'creative',
    timer: {
        duration: 0,
        endTime: null,
        isActive: false
    },
    selectedMemeTemplateId: null,
    hasSubmittedMeme: false,
    playerSubmission: null,
    currentMemeToRate: null,
    memesRatedThisRound: [],
    showScoreboard: false,
    finalScores: [],
    isLoading: false,
    error: null,
    lastUpdated: new Date().toISOString(),
    version: 1
};

/**
 * Interface for persisted state in session storage
 */
export interface PersistedGameState extends GameState {
    persistedAt: string;
}
