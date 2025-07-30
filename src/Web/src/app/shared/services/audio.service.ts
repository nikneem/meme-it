import { Injectable, inject, signal, effect } from '@angular/core';
import { SettingsService } from './settings.service';

@Injectable({
  providedIn: 'root'
})
export class AudioService {
  private settingsService = inject(SettingsService);
  private backgroundMusic: HTMLAudioElement | null = null;
  
  private readonly BACKGROUND_MUSIC_URL = './music/background-music.mp3';

  constructor() {
    this.initializeBackgroundMusic();
    
    // React to settings changes
    effect(() => {
      const settings = this.settingsService.settings();
      this.updateBackgroundMusic(settings.musicVolume, settings.musicMuted);
    });
  }

  private initializeBackgroundMusic(): void {
    this.backgroundMusic = new Audio(this.BACKGROUND_MUSIC_URL);
    this.backgroundMusic.loop = true;
    this.backgroundMusic.preload = 'auto';
    
    // Set initial volume and mute state
    const settings = this.settingsService.settings();
    this.updateBackgroundMusic(settings.musicVolume, settings.musicMuted);
  }

  private updateBackgroundMusic(volume: number, muted: boolean): void {
    if (this.backgroundMusic) {
      this.backgroundMusic.volume = muted ? 0 : volume / 100;
      
      // Auto-play background music if not muted and volume > 0
      if (!muted && volume > 0) {
        this.playBackgroundMusic();
      } else {
        this.pauseBackgroundMusic();
      }
    }
  }

  playBackgroundMusic(): void {
    if (this.backgroundMusic && this.backgroundMusic.paused) {
      this.backgroundMusic.play().catch(error => {
        console.warn('Failed to play background music:', error);
      });
    }
  }

  pauseBackgroundMusic(): void {
    if (this.backgroundMusic && !this.backgroundMusic.paused) {
      this.backgroundMusic.pause();
    }
  }

  playSoundEffect(soundUrl: string): void {
    const settings = this.settingsService.settings();
    
    if (settings.soundEffectsMuted || settings.soundEffectsVolume === 0) {
      return;
    }

    const audio = new Audio(soundUrl);
    audio.volume = settings.soundEffectsVolume / 100;
    audio.play().catch(error => {
      console.warn('Failed to play sound effect:', error);
    });
  }

  // Method to handle user interaction (required for autoplay)
  enableAudio(): void {
    if (this.backgroundMusic) {
      const settings = this.settingsService.settings();
      if (!settings.musicMuted && settings.musicVolume > 0) {
        this.playBackgroundMusic();
      }
    }
  }
}
