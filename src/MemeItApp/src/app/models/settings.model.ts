export interface SoundSettings {
    musicEnabled: boolean;
    musicVolume: number;
    soundEffectsEnabled: boolean;
    soundEffectsVolume: number;
}

export interface UserSettings {
    displayName: string;
    language: string;
    autoReady: boolean;
}

export interface AppSettings {
    sound: SoundSettings;
    user: UserSettings;
}

export const DEFAULT_SOUND_SETTINGS: SoundSettings = {
    musicEnabled: true,
    musicVolume: 40,
    soundEffectsEnabled: true,
    soundEffectsVolume: 80
};

export const DEFAULT_USER_SETTINGS: UserSettings = {
    displayName: '',
    language: 'en',
    autoReady: false
};

export const DEFAULT_APP_SETTINGS: AppSettings = {
    sound: DEFAULT_SOUND_SETTINGS,
    user: DEFAULT_USER_SETTINGS
};
