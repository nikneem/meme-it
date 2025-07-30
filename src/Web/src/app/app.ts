import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService } from './shared/services/settings.service';

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

  ngOnInit(): void {
    // Initialize translation service with user's preferred language
    const currentLanguage = this.settingsService.settings().language;
    this.translateService.setDefaultLang('en');
    this.translateService.use(currentLanguage);
  }
}
