import { Component, OnInit, OnDestroy, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, Subject, interval, takeUntil, take, startWith, map, switchMap, of, firstValueFrom } from 'rxjs';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressBarModule } from 'primeng/progressbar';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';

import { Game } from '../../store/game/game.models';
import { MemeTemplate } from '../../shared/models/meme.models';
import { selectCurrentGame } from '../../store/game/game.selectors';
import { selectCurrentPlayer } from '../../store/player/player.selectors';
import { GamePersistenceService } from '../../shared/services/game-persistence.service';
import { GameService } from '../../services/game.service';

export interface MemeTextInput {
  id: number;
  value: string;
  maxLength: number;
}

export interface GameRoundState {
  currentRound: number;
  totalRounds: number;
  timeRemaining: number;
  totalTime: number;
  isCompleted: boolean;
  startTime: number;
}

export interface SavePlayerMemeRequest {
  gameCode: string;
  memeTemplateId: string;
  texts: string[];
  roundNumber: number;
}

export interface SavePlayerMemeResponse {
  id: string;
  gameCode: string;
  playerId: string;
  memeTemplateId: string;
  texts: string[];
  roundNumber: number;
  createdAt: Date;
}

@Component({
  selector: 'app-game-active',
  standalone: true,
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    ProgressBarModule,
    TagModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './game-active.component.html',
  styleUrl: './game-active.component.scss'
})
export class GameActiveComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private timer$ = new Subject<void>();
  private readonly router = inject(Router);
  private readonly store = inject(Store);
  private readonly gameService = inject(GameService);
  private readonly messageService = inject(MessageService);
  private readonly gamePersistenceService = inject(GamePersistenceService);

  // Make Math available in template
  Math = Math;

  // Observables
  currentGame$: Observable<Game | null>;
  currentPlayer$: Observable<any>;

  // Signals for reactive state
  currentMeme = signal<MemeTemplate | null>(null);
  memeTextInputs = signal<MemeTextInput[]>([]);
  roundState = signal<GameRoundState>({
    currentRound: 1,
    totalRounds: 5,
    timeRemaining: 45,
    totalTime: 45,
    isCompleted: false,
    startTime: Date.now()
  });
  isLoading = signal(false);
  isSaving = signal(false);

  // Computed values
  timePercentage = computed(() => {
    const state = this.roundState();
    return Math.max(0, (state.timeRemaining / state.totalTime) * 100);
  });

  timeDisplay = computed(() => {
    const remaining = this.roundState().timeRemaining;
    return `${Math.floor(remaining / 60)}:${(remaining % 60).toString().padStart(2, '0')}`;
  });

  canSaveMeme = computed(() => {
    const inputs = this.memeTextInputs();
    return inputs.length > 0 && inputs.some(input => input.value.trim().length > 0) && !this.isSaving();
  });

  constructor() {
    this.currentGame$ = this.store.select(selectCurrentGame);
    this.currentPlayer$ = this.store.select(selectCurrentPlayer);

    // Effect to handle game state changes
    effect(() => {
      const game = this.getCurrentGameSync();
      if (game) {
        this.updateRoundStateFromGame(game);
      }
    });
  }

  ngOnInit() {
    this.initializeGameRound();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    this.timer$.next();
    this.timer$.complete();
  }

  private getCurrentGameSync(): Game | null {
    let currentGame: Game | null = null;
    this.currentGame$.pipe(take(1)).subscribe(game => currentGame = game);
    return currentGame;
  }

  private updateRoundStateFromGame(game: Game): void {
    const newState: GameRoundState = {
      currentRound: 1, // TODO: Get from game state when rounds are implemented
      totalRounds: game.settings.totalRounds,
      timeRemaining: this.calculateTimeRemaining(game),
      totalTime: game.settings.timePerRound,
      isCompleted: false,
      startTime: Date.now() // TODO: Get actual round start time from game state
    };

    this.roundState.set(newState);
  }

  private calculateTimeRemaining(game: Game): number {
    // TODO: Calculate based on actual round start time from server
    // For now, check if we have persisted timer state
    const persistedState = this.gamePersistenceService.getRoundTimerState();
    if (persistedState && persistedState.gameCode === game.code) {
      const elapsed = (Date.now() - persistedState.startTime) / 1000;
      return Math.max(0, game.settings.timePerRound - elapsed);
    }
    return game.settings.timePerRound;
  }

  private async initializeGameRound(): Promise<void> {
    try {
      this.isLoading.set(true);

      // Load a random meme for this round
      await this.loadRandomMeme();

      // Start the timer
      this.startRoundTimer();

      // Persist the round start time
      const game = this.getCurrentGameSync();
      if (game) {
        this.gamePersistenceService.saveRoundTimerState({
          gameCode: game.code,
          roundNumber: 1, // TODO: Get actual round number
          startTime: Date.now(),
          timePerRound: game.settings.timePerRound
        });
      }

    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to initialize game round'
      });
      console.error('Failed to initialize game round:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async loadRandomMeme(): Promise<void> {
    try {
      // Get current game and player from observables
      const game = await firstValueFrom(this.currentGame$);
      const player = await firstValueFrom(this.currentPlayer$);
      
      if (!game || !player) {
        console.error('Game or player not available for loading meme');
        return;
      }

      // Call server endpoint to get random meme for this player
      const response = await firstValueFrom(this.gameService.getRandomMemeForPlayer(game.code, player.id));
      
      if (response && response.memeTemplate) {
        const meme: MemeTemplate = {
          id: response.memeTemplate.id,
          name: response.memeTemplate.name,
          description: response.memeTemplate.description,
          sourceImageUrl: response.memeTemplate.imageUrl,
          sourceWidth: response.memeTemplate.sourceWidth,
          sourceHeight: response.memeTemplate.sourceHeight,
          textAreas: response.memeTemplate.textAreas.map((ta: any) => ({
            id: ta.id,
            x: ta.x,
            y: ta.y,
            width: ta.width,
            height: ta.height,
            maxLength: ta.maxLength,
            fontSize: ta.fontSize,
            fontFamily: ta.fontFamily,
            color: ta.color,
            isBold: ta.isBold,
            isItalic: ta.isItalic || false,
            isUnderline: ta.isUnderline || false,
            textAlign: ta.textAlign || 'left',
            verticalAlign: ta.verticalAlign || 'middle'
          }))
        };

        this.currentMeme.set(meme);
        
        // Initialize text inputs based on meme text areas
        const textInputs: MemeTextInput[] = meme.textAreas.map((textArea: any, index: number) => ({
          id: index,
          value: '',
          maxLength: textArea.maxLength || 100
        }));
        
        this.memeTextInputs.set(textInputs);
      }
    } catch (error) {
      console.error('Failed to load random meme:', error);
      throw error;
    }
  }

  private startRoundTimer(): void {
    // Stop any existing timer
    this.timer$.next();

    const startTime = Date.now();
    const game = this.getCurrentGameSync();
    const totalTime = game?.settings.timePerRound || 45;

    interval(1000).pipe(
      startWith(0),
      map(() => {
        const elapsed = (Date.now() - startTime) / 1000;
        return Math.max(0, totalTime - elapsed);
      }),
      takeUntil(this.timer$),
      takeUntil(this.destroy$)
    ).subscribe(timeRemaining => {
      const currentState = this.roundState();
      const newState: GameRoundState = {
        ...currentState,
        timeRemaining: Math.floor(timeRemaining),
        isCompleted: timeRemaining <= 0
      };

      this.roundState.set(newState);

      // Auto-save when time runs out
      if (timeRemaining <= 0 && !currentState.isCompleted) {
        this.autoSaveMeme();
      }
    });
  }

  onTextInputChange(inputId: number, value: string): void {
    const inputs = this.memeTextInputs();
    const updatedInputs = inputs.map(input => 
      input.id === inputId 
        ? { ...input, value } 
        : input
    );
    this.memeTextInputs.set(updatedInputs);
  }

  getMemeTextInput(index: number): MemeTextInput | undefined {
    return this.memeTextInputs()[index];
  }

  async saveMeme(): Promise<void> {
    if (!this.canSaveMeme()) return;

    try {
      this.isSaving.set(true);

      const game = this.getCurrentGameSync();
      const meme = this.currentMeme();
      const textInputs = this.memeTextInputs();

      if (!game || !meme) {
        throw new Error('Missing game or meme data');
      }

      // Prepare meme data for saving
      const memeTexts = textInputs
        .filter(input => input.value.trim().length > 0)
        .map(input => input.value.trim());

      // TODO: Call API to save completed meme
      console.log('Saving meme:', {
        gameCode: game.code,
        memeTemplateId: meme.id,
        texts: memeTexts,
        roundNumber: this.roundState().currentRound
      });

      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Your meme has been saved!'
      });

      // Mark round as completed
      const currentState = this.roundState();
      this.roundState.set({
        ...currentState,
        isCompleted: true
      });

      // Stop the timer
      this.timer$.next();

      // TODO: Navigate to next round or voting phase
      // For now, just show completion message

    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to save your meme. Please try again.'
      });
      console.error('Failed to save meme:', error);
    } finally {
      this.isSaving.set(false);
    }
  }

  private async autoSaveMeme(): Promise<void> {
    const textInputs = this.memeTextInputs();
    const hasText = textInputs.some(input => input.value.trim().length > 0);
    
    if (hasText) {
      await this.saveMeme();
    } else {
      // No text entered, just mark as completed
      const currentState = this.roundState();
      this.roundState.set({
        ...currentState,
        isCompleted: true
      });

      this.messageService.add({
        severity: 'info',
        summary: 'Time\'s up!',
        detail: 'You didn\'t enter any text for this round.'
      });
    }
  }

  onLeaveGame(): void {
    // TODO: Implement leave game logic
    this.router.navigate(['/home']);
  }
}
