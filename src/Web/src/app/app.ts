import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { Store } from '@ngrx/store';
import { SettingsService } from './shared/services/settings.service';
import { GamePersistenceService } from './shared/services/game-persistence.service';
import { refreshGameStateFromServer } from './store/game/game.actions';

@Component({
  selector: 'meme-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected title = 'meme-it';
  
  private translateService = inject(TranslateService);
  private settingsService = inject(SettingsService);
  private store = inject(Store);
  private gamePersistenceService = inject(GamePersistenceService);

  ngOnInit(): void {
    // Initialize translation service with user's preferred language
    const currentLanguage = this.settingsService.settings().language;
    this.translateService.setDefaultLang('en');
    this.translateService.use(currentLanguage);

    // Try to restore game state on app initialization
    if (this.gamePersistenceService.hasValidGameState()) {
      const persistedState = this.gamePersistenceService.loadGameState();
      
      if (persistedState && persistedState.gameCode && persistedState.playerId && persistedState.playerName) {
        this.store.dispatch(refreshGameStateFromServer({
          gameCode: persistedState.gameCode,
          playerId: persistedState.playerId,
          playerName: persistedState.playerName
        }));
      }
    }
  }
}
