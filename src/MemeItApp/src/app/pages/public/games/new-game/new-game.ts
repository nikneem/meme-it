import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@services/auth.service';
import { GameService } from '@services/game.service';

@Component({
  selector: 'memeit-new-game',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './new-game.html',
  styleUrl: './new-game.scss',
})
export class NewGamePage implements OnInit {
  gameForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  private readonly DISPLAY_NAME_KEY = 'memeit_displayName';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private gameService: GameService,
    private router: Router
  ) {
    this.gameForm = this.fb.group({
      displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20)]],
      password: ['', [Validators.maxLength(50)]]
    });
  }

  ngOnInit(): void {
    const savedDisplayName = localStorage.getItem(this.DISPLAY_NAME_KEY);
    if (savedDisplayName) {
      this.gameForm.patchValue({
        displayName: savedDisplayName
      });
    }
  }

  onSubmit(): void {
    if (this.gameForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const displayName = this.gameForm.value.displayName;
    const password = this.gameForm.value.password || undefined;

    // First, request JWT token
    this.authService.requestToken({ displayName }).subscribe({
      next: () => {
        // Save display name to local storage
        localStorage.setItem(this.DISPLAY_NAME_KEY, displayName);
        // Then create the game
        this.gameService.createGame({ password }).subscribe({
          next: (game) => {
            // Navigate to lobby
            this.router.navigate(['/app/games', game.gameCode]);
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = 'Failed to create game. Please try again.';
            console.error('Game creation error:', error);
          }
        });
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to authenticate. Please try again.';
        console.error('Authentication error:', error);
      }
    });
  }
}
