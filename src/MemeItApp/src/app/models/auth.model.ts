export interface AuthTokenRequest {
  displayName: string;
  gameCode?: string;
}

export interface AuthTokenResponse {
  token: string;
  expiresAt: string;
}
