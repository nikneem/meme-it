import { Injectable, signal } from '@angular/core';

export interface UserSettings {
  musicVolume: number;
  soundEffectsVolume: number;
  musicMuted: boolean;
  soundEffectsMuted: boolean;
  language: string;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly STORAGE_KEY = 'meme-it-settings';
  
  private readonly defaultSettings: UserSettings = {
    musicVolume: 50,
    soundEffectsVolume: 50,
    musicMuted: false,
    soundEffectsMuted: false,
    language: 'en'
  };

  private settingsSignal = signal<UserSettings>(this.loadSettings());

  get settings() {
    return this.settingsSignal.asReadonly();
  }

  updateSettings(newSettings: Partial<UserSettings>): void {
    const currentSettings = this.settingsSignal();
    const updatedSettings = { ...currentSettings, ...newSettings };
    this.settingsSignal.set(updatedSettings);
    this.saveSettings(updatedSettings);
  }

  private loadSettings(): UserSettings {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEY);
      if (stored) {
        const parsed = JSON.parse(stored);
        return { ...this.defaultSettings, ...parsed };
      }
    } catch (error) {
      console.warn('Failed to load settings from localStorage:', error);
    }
    return this.defaultSettings;
  }

  private saveSettings(settings: UserSettings): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(settings));
    } catch (error) {
      console.warn('Failed to save settings to localStorage:', error);
    }
  }
}
