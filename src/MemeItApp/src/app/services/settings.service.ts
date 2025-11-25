import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {
    AppSettings,
    SoundSettings,
    UserSettings,
    DEFAULT_APP_SETTINGS
} from '../models/settings.model';

@Injectable({
    providedIn: 'root'
})
export class SettingsService {
    private readonly STORAGE_KEY = 'memeit-settings';

    private settingsSubject: BehaviorSubject<AppSettings>;
    public settings$: Observable<AppSettings>;

    constructor() {
        const storedSettings = this.loadSettings();
        this.settingsSubject = new BehaviorSubject<AppSettings>(storedSettings);
        this.settings$ = this.settingsSubject.asObservable();
    }

    private loadSettings(): AppSettings {
        try {
            const stored = localStorage.getItem(this.STORAGE_KEY);
            if (stored) {
                const parsed = JSON.parse(stored);
                // Merge with defaults to ensure all properties exist
                return {
                    sound: { ...DEFAULT_APP_SETTINGS.sound, ...parsed.sound },
                    user: { ...DEFAULT_APP_SETTINGS.user, ...parsed.user }
                };
            }
        } catch (error) {
            console.error('Error loading settings from localStorage:', error);
        }
        return DEFAULT_APP_SETTINGS;
    }

    private saveSettings(settings: AppSettings): void {
        try {
            localStorage.setItem(this.STORAGE_KEY, JSON.stringify(settings));
            this.settingsSubject.next(settings);
        } catch (error) {
            console.error('Error saving settings to localStorage:', error);
        }
    }

    getCurrentSettings(): AppSettings {
        return this.settingsSubject.value;
    }

    updateSoundSettings(soundSettings: Partial<SoundSettings>): void {
        const currentSettings = this.getCurrentSettings();
        const updatedSettings: AppSettings = {
            ...currentSettings,
            sound: { ...currentSettings.sound, ...soundSettings }
        };
        this.saveSettings(updatedSettings);
    }

    updateUserSettings(userSettings: Partial<UserSettings>): void {
        const currentSettings = this.getCurrentSettings();
        const updatedSettings: AppSettings = {
            ...currentSettings,
            user: { ...currentSettings.user, ...userSettings }
        };
        this.saveSettings(updatedSettings);
    }

    resetToDefaults(): void {
        this.saveSettings(DEFAULT_APP_SETTINGS);
    }
}
