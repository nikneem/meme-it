import { Component, signal, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { SettingsService, UserSettings } from '../../services/settings.service';

interface LanguageOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-config-panel',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule
  ],
  templateUrl: './config-panel.component.html',
  styleUrl: './config-panel.component.scss'
})
export class ConfigPanelComponent {
  private settingsService = inject(SettingsService);
  private translateService = inject(TranslateService);

  panelVisible = signal(false);
  currentSettings = this.settingsService.settings;

  languageOptions: LanguageOption[] = [
    { label: 'English', value: 'en' },
    { label: 'Nederlands', value: 'nl' }
  ];

  constructor() {
    // Update language labels when language changes
    effect(() => {
      const currentLanguage = this.currentSettings().language;
      this.translateService.use(currentLanguage);
      this.updateLanguageLabels();
    });
  }

  togglePanel(): void {
    this.panelVisible.update(visible => !visible);
  }

  onOverlayClick(event: Event): void {
    this.panelVisible.set(false);
  }

  onLanguageChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.settingsService.updateSettings({ language: target.value });
  }

  onMusicVolumeChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.settingsService.updateSettings({ musicVolume: +target.value });
  }

  onSoundEffectsVolumeChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.settingsService.updateSettings({ soundEffectsVolume: +target.value });
  }

  onMusicMuteChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.settingsService.updateSettings({ musicMuted: target.checked });
  }

  onSoundEffectsMuteChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.settingsService.updateSettings({ soundEffectsMuted: target.checked });
  }

  private updateLanguageLabels(): void {
    this.translateService.get(['languages.en', 'languages.nl']).subscribe(translations => {
      this.languageOptions = [
        { label: translations['languages.en'], value: 'en' },
        { label: translations['languages.nl'], value: 'nl' }
      ];
    });
  }
}
