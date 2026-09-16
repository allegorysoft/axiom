export interface OAuthToken {
  accessToken: string;
  refreshToken?: string;
  expiresAt?: number;
}

const STORAGE_KEY = 'auth_token';

export interface OAuthStorage {
  get(): OAuthToken | null;
  set(token: OAuthToken): void;
  remove(key: string): void;
  clear(): void;
  hasToken(): boolean;
}

export const oAuthStorage: OAuthStorage = {
  get(): OAuthToken | null {
    const value = localStorage.getItem(STORAGE_KEY);

    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as OAuthToken;
    } catch {
      return null;
    }
  },
  set(token: OAuthToken): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(token));
  },
  remove(key: string): void {
    localStorage.removeItem(key);
  },
  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
  },
  hasToken(): boolean {
    return this.get() !== null;
  },
};
