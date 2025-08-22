import { ApplicationConfig } from '@angular/core';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { gameReducer } from './game/game.reducer';
import { playerReducer } from './player/player.reducer';
import { GameEffects } from './game/game.effects';

export const appStoreConfig: ApplicationConfig['providers'] = [
  provideStore({
    game: gameReducer,
    player: playerReducer
  }),
  provideEffects([GameEffects]),
  provideStoreDevtools({
    maxAge: 25,
    logOnly: false,
    autoPause: true,
    trace: false,
    traceLimit: 75
  })
];
