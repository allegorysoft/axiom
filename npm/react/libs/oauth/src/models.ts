import { type OAuthToken, AxiomStore } from '@axiomframework/react-core';

export interface OAuthState {
  token: OAuthToken | null;
}

export interface OAuthStore extends AxiomStore<OAuthState> {
  setToken(token: OAuthToken): void;
  clear(): void;
}
