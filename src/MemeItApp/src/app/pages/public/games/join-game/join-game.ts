import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '@services/auth.service';
import { GameService } from '@services/game.service';
import { SettingsService } from '@services/settings.service';

@Component({
  selector: 'memeit-join-game',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './join-game.html',
  styleUrl: './join-game.scss',
})
export class JoinGamePage implements OnInit {
  gameForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private gameService: GameService,
    private settingsService: SettingsService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.gameForm = this.fb.group({
      displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(20)]],
      gameCode: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(10)]],
      password: ['', [Validators.maxLength(50)]]
    });
  }

  ngOnInit(): void {
    const gamecode = this.route.snapshot.paramMap.get('gamecode');
    const settings = this.settingsService.getCurrentSettings();

    this.gameForm.patchValue({
      ...(gamecode && { gameCode: gamecode.toUpperCase() }),
      ...(settings.user.displayName && { displayName: settings.user.displayName })
    });
  }

  onSubmit(): void {
    if (this.gameForm.invalid) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const displayName = this.gameForm.value.displayName;
    const gameCode = this.gameForm.value.gameCode.toUpperCase();
    const password = this.gameForm.value.password || undefined;

    // First, request JWT token
    this.authService.requestToken({ displayName, gameCode }).subscribe({
      next: () => {
        // Save display name to settings
        this.settingsService.updateUserSettings({ displayName });
        // Then join the game
        this.gameService.joinGame({ gameCode, password }).subscribe({
          next: (game) => {
            // Navigate to lobby
            this.router.navigate(['/app/games', game.gameCode]);
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = 'Failed to join game. Please check the game code and password.';
            console.error('Game join error:', error);
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
