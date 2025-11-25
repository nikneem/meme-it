import { Injectable } from '@angular/core';
import { SettingsService } from './settings.service';
import { SoundSettings } from '../models/settings.model';

@Injectable({
    providedIn: 'root'
})
export class AudioService {
    private backgroundMusic: HTMLAudioElement | null = null;
    private currentSettings: SoundSettings;
    private userInteracted = false;
    private playAttempted = false;

    constructor(private settingsService: SettingsService) {
        this.currentSettings = this.settingsService.getCurrentSettings().sound;

        // Subscribe to settings changes
        this.settingsService.settings$.subscribe(settings => {
            this.currentSettings = settings.sound;
            this.updateAudio();
        });

        this.initializeBackgroundMusic();
        this.setupUserInteractionListener();
    }

    private initializeBackgroundMusic(): void {
        this.backgroundMusic = new Audio('music/background-music.mp3');
        this.backgroundMusic.loop = true;
        this.backgroundMusic.volume = this.currentSettings.musicVolume / 100;
    }

    private setupUserInteractionListener(): void {
        // Listen for first user interaction to start music
        const startMusicOnInteraction = () => {
            if (!this.userInteracted) {
                this.userInteracted = true;
                if (this.currentSettings.musicEnabled && !this.playAttempted) {
                    this.playBackgroundMusic();
                }
                // Remove listeners after first interaction
                document.removeEventListener('click', startMusicOnInteraction);
                document.removeEventListener('keydown', startMusicOnInteraction);
                document.removeEventListener('touchstart', startMusicOnInteraction);
            }
        };

        document.addEventListener('click', startMusicOnInteraction);
        document.addEventListener('keydown', startMusicOnInteraction);
        document.addEventListener('touchstart', startMusicOnInteraction);
    }

    private updateAudio(): void {
        if (!this.backgroundMusic) return;

        // Update music volume and playback state
        this.backgroundMusic.volume = this.currentSettings.musicVolume / 100;

        if (this.currentSettings.musicEnabled) {
            this.playBackgroundMusic();
        } else {
            this.pauseBackgroundMusic();
        }
    }

    private playBackgroundMusic(): void {
        if (!this.backgroundMusic) return;

        this.playAttempted = true;
        const playPromise = this.backgroundMusic.play();
        if (playPromise !== undefined) {
            playPromise.catch(error => {
                // Auto-play was prevented, which is common on page load
                console.log('Background music auto-play prevented. Will try on user interaction:', error);
                this.playAttempted = false; // Allow retry on user interaction
            });
        }
    }

    private pauseBackgroundMusic(): void {
        if (this.backgroundMusic && !this.backgroundMusic.paused) {
            this.backgroundMusic.pause();
        }
    }

    // Method to manually start music (useful for user-initiated actions)
    public startMusic(): void {
        if (this.currentSettings.musicEnabled) {
            this.playBackgroundMusic();
        }
    }

    // Play a sound effect
    public playSoundEffect(soundPath: string): void {
        if (!this.currentSettings.soundEffectsEnabled) return;

        const soundEffect = new Audio(soundPath);
        soundEffect.volume = this.currentSettings.soundEffectsVolume / 100;

        const playPromise = soundEffect.play();
        if (playPromise !== undefined) {
            playPromise.catch(error => {
                console.log('Sound effect play failed:', error);
            });
        }
    }

    public cleanup(): void {
        if (this.backgroundMusic) {
            this.backgroundMusic.pause();
            this.backgroundMusic = null;
        }
    }
}
