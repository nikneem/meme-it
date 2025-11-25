import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSliderModule } from '@angular/material/slider';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { trigger, transition, style, animate } from '@angular/animations';
import { SettingsService } from '../../services/settings.service';
import { AudioService } from '../../services/audio.service';
import { AppSettings } from '../../models/settings.model';

@Component({
    selector: 'memeit-settings',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        MatButtonModule,
        MatIconModule,
        MatSliderModule,
        MatCheckboxModule,
        MatInputModule,
        MatFormFieldModule,
        MatSelectModule,
        MatCardModule
    ],
    templateUrl: './settings.component.html',
    styleUrl: './settings.component.scss',
    animations: [
        trigger('slidePanel', [
            transition(':enter', [
                style({ transform: 'translateX(100%)', opacity: 0 }),
                animate('300ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
            ]),
            transition(':leave', [
                animate('200ms ease-in', style({ transform: 'translateX(100%)', opacity: 0 }))
            ])
        ])
    ]
})
export class SettingsComponent implements OnInit {
    protected showSoundPanel = signal(false);
    protected showSettingsPanel = signal(false);
    protected settings = signal<AppSettings | null>(null);

    protected languages = [
        { code: 'en', name: 'English' }
    ];

    constructor(
        private settingsService: SettingsService,
        private audioService: AudioService
    ) { }

    ngOnInit(): void {
        this.settingsService.settings$.subscribe(settings => {
            this.settings.set(settings);
        });
    }

    protected toggleSoundPanel(): void {
        this.showSoundPanel.set(!this.showSoundPanel());
        if (this.showSoundPanel()) {
            this.showSettingsPanel.set(false);
        }
    }

    protected toggleSettingsPanel(): void {
        this.showSettingsPanel.set(!this.showSettingsPanel());
        if (this.showSettingsPanel()) {
            this.showSoundPanel.set(false);
        }
    }

    protected closePanels(): void {
        this.showSoundPanel.set(false);
        this.showSettingsPanel.set(false);
    }

    protected onMusicEnabledChange(enabled: boolean): void {
        this.settingsService.updateSoundSettings({ musicEnabled: enabled });
        if (enabled) {
            this.audioService.startMusic();
        }
    }

    protected onMusicVolumeChange(volume: number): void {
        this.settingsService.updateSoundSettings({ musicVolume: volume });
    }

    protected onSoundEffectsEnabledChange(enabled: boolean): void {
        this.settingsService.updateSoundSettings({ soundEffectsEnabled: enabled });
    }

    protected onSoundEffectsVolumeChange(volume: number): void {
        this.settingsService.updateSoundSettings({ soundEffectsVolume: volume });
    }

    protected onDisplayNameChange(displayName: string): void {
        this.settingsService.updateUserSettings({ displayName });
    }

    protected onLanguageChange(language: string): void {
        this.settingsService.updateUserSettings({ language });
    }

    protected onAutoReadyChange(autoReady: boolean): void {
        this.settingsService.updateUserSettings({ autoReady });
    }
}
