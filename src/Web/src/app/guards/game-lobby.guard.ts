import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Observable, map, take } from 'rxjs';
import { selectIsInLobby } from '../store/game/game.selectors';

@Injectable({
  providedIn: 'root'
})
export class GameLobbyGuard implements CanActivate {
  
  constructor(
    private store: Store,
    private router: Router
  ) {}

  canActivate(): Observable<boolean> {
    return this.store.select(selectIsInLobby).pipe(
      take(1),
      map(isInLobby => {
        if (!isInLobby) {
          this.router.navigate(['/home']);
          return false;
        }
        return true;
      })
    );
  }
}
