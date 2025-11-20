import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { AuthTokenRequest, AuthTokenResponse } from '../models/auth.model';
import { AUTH_TOKEN_KEY } from '../constants/auth.constants';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'http://localhost:5000/users';

  constructor(private http: HttpClient) { }

  requestToken(request: AuthTokenRequest): Observable<AuthTokenResponse> {
    return this.http.post<AuthTokenResponse>(`${this.apiUrl}`, request).pipe(
      tap(response => this.storeToken(response.token))
    );
  }

  storeToken(token: string): void {
    sessionStorage.setItem(AUTH_TOKEN_KEY, token);
  }

  getToken(): string | null {
    return sessionStorage.getItem(AUTH_TOKEN_KEY);
  }

  clearToken(): void {
    sessionStorage.removeItem(AUTH_TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getCurrentUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.sub || null;
    } catch (error) {
      console.error('Failed to decode token:', error);
      return null;
    }
  }
}
