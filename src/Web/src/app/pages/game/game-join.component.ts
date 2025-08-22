import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';

import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { DividerModule } from 'primeng/divider';

import { createGame, joinGame } from '../../store/game/game.actions';
import { selectIsLoading, selectGameError } from '../../store/game/game.selectors';

@Component({
  selector: 'app-game-join',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    DividerModule
  ],
  templateUrl: './game-join.component.html',
  styleUrl: './game-join.component.scss'
})
export class GameJoinComponent implements OnInit {
  createGameForm!: FormGroup;
  joinGameForm!: FormGroup;
  activeTab = 'create';
  
  isLoading$: Observable<boolean>;
  error$: Observable<string | null>;

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private route: ActivatedRoute
  ) {
    this.isLoading$ = this.store.select(selectIsLoading);
    this.error$ = this.store.select(selectGameError);
  }

  ngOnInit() {
    this.initializeForms();
    this.setInitialTabFromQueryParams();
  }

  private setInitialTabFromQueryParams() {
    const tab = this.route.snapshot.queryParams['tab'];
    if (tab === 'create' || tab === 'join') {
      this.activeTab = tab;
    } else {
      // Default to 'create' if no valid tab parameter is provided
      this.activeTab = 'create';
    }
  }

  private initializeForms() {
    this.createGameForm = this.fb.group({
      playerName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['']
    });

    this.joinGameForm = this.fb.group({
      gameCode: ['', [Validators.required, Validators.minLength(3)]],
      playerName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['']
    });
  }

  onCreateGame() {
    if (this.createGameForm.valid) {
      const formValue = this.createGameForm.value;
      this.store.dispatch(createGame({
        request: {
          playerName: formValue.playerName,
          password: formValue.password || undefined
        }
      }));
    }
  }

  onJoinGame() {
    if (this.joinGameForm.valid) {
      const formValue = this.joinGameForm.value;
      this.store.dispatch(joinGame({
        request: {
          gameCode: formValue.gameCode.toUpperCase(),
          playerName: formValue.playerName,
          password: formValue.password || undefined
        }
      }));
    }
  }
}
