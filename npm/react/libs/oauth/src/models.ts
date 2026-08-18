import type { OAuthToken } from '@axiomframework/react-core';
import { AxiomStore } from '../../core/src/models/common';

export interface OAuthState {
  token: OAuthToken | null;
}

export interface OAuthStore extends AxiomStore<OAuthState> {
  setToken(token: OAuthToken): void;
  clear(): void;
}
