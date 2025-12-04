import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PasscodeAuthService } from '@services/passcode-auth.service';

@Component({
    selector: 'app-management-login',
    imports: [
        CommonModule,
        FormsModule,
        MatCardModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule
    ],
    template: `
        <div class="login-container">
            <mat-card class="login-card">
                <mat-card-header>
                    <mat-card-title>
                        <mat-icon>lock</mat-icon>
                        Management Access
                    </mat-card-title>
                    <mat-card-subtitle>Enter passcode to continue</mat-card-subtitle>
                </mat-card-header>
                <mat-card-content>
                    <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
                        <mat-form-field appearance="outline" class="full-width">
                            <mat-label>Passcode</mat-label>
                            <input 
                                matInput 
                                type="password" 
                                [(ngModel)]="passcode" 
                                name="passcode"
                                placeholder="Enter management passcode"
                                required
                                autofocus
                                (keyup.enter)="onSubmit()"
                            />
                            <mat-icon matSuffix>key</mat-icon>
                        </mat-form-field>
                        
                        @if (errorMessage()) {
                            <div class="error-message">
                                <mat-icon>error</mat-icon>
                                {{ errorMessage() }}
                            </div>
                        }
                    </form>
                </mat-card-content>
                <mat-card-actions>
                    <button 
                        mat-raised-button 
                        color="primary" 
                        (click)="onSubmit()"
                        [disabled]="!passcode || isLoading()"
                        class="full-width"
                    >
                        {{ isLoading() ? 'Checking...' : 'Access Management' }}
                    </button>
                    <button 
                        mat-button 
                        color="accent" 
                        (click)="goBack()"
                        class="full-width"
                    >
                        Cancel
                    </button>
                </mat-card-actions>
            </mat-card>
        </div>
    `,
    styles: [`
        .login-container {
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 1rem;
        }

        .login-card {
            width: 100%;
            max-width: 400px;
            
            mat-card-header {
                display: flex;
                flex-direction: column;
                align-items: center;
                padding: 1.5rem 0 1rem;
                
                mat-card-title {
                    display: flex;
                    align-items: center;
                    gap: 0.5rem;
                    font-size: 1.5rem;
                    margin-bottom: 0.5rem;
                    
                    mat-icon {
                        font-size: 2rem;
                        width: 2rem;
                        height: 2rem;
                    }
                }
                
                mat-card-subtitle {
                    text-align: center;
                }
            }
            
            mat-card-content {
                padding: 1.5rem;
                
                form {
                    display: flex;
                    flex-direction: column;
                }
            }
            
            mat-card-actions {
                display: flex;
                flex-direction: column;
                gap: 0.5rem;
                padding: 0 1.5rem 1.5rem;
            }
        }

        .full-width {
            width: 100%;
        }

        .error-message {
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: #f44336;
            background: #ffebee;
            padding: 0.75rem;
            border-radius: 4px;
            margin-top: 1rem;
            
            mat-icon {
                font-size: 1.25rem;
                width: 1.25rem;
                height: 1.25rem;
            }
        }
    `]
})
export class ManagementLoginPage {
    passcode = '';
    errorMessage = signal<string>('');
    isLoading = signal<boolean>(false);

    constructor(
        private passcodeAuthService: PasscodeAuthService,
        private router: Router
    ) { }

    onSubmit(): void {
        if (!this.passcode) {
            return;
        }

        this.isLoading.set(true);
        this.errorMessage.set('');

        // Store the passcode - server will validate via interceptor
        this.passcodeAuthService.setPasscode(this.passcode);

        // Navigate to management page
        // If the API key is invalid, the server will reject requests
        this.router.navigate(['/management/memes']);
    }

    goBack(): void {
        this.router.navigate(['/']);
    }
}
