import { type OAuthToken, createStore } from '@axiomframework/react-core';
import type { OAuthState, OAuthStore } from './models';

const initialState: OAuthState = {
  token: null,
};

const baseStore = createStore<OAuthState>(initialState);

export const oAuthStore: OAuthStore = Object.assign(baseStore, {
  setToken(token: OAuthToken): void {
    baseStore.set(() => ({
      token,
    }));
  },
  clear(): void {
    baseStore.set(() => ({ ...initialState }));
  },
});
