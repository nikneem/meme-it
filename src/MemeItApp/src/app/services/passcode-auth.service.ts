import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class PasscodeAuthService {
    private readonly STORAGE_KEY = 'meme_it_management_passcode';

    setPasscode(passcode: string): void {
        sessionStorage.setItem(this.STORAGE_KEY, passcode);
    }

    getPasscode(): string | null {
        return sessionStorage.getItem(this.STORAGE_KEY);
    }

    isAuthenticated(): boolean {
        return this.getPasscode() !== null;
    }

    clearPasscode(): void {
        sessionStorage.removeItem(this.STORAGE_KEY);
    }
}
