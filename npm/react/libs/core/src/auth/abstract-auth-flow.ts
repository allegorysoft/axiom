import type { OAuth } from '../models/oauth';
import { type OAuthStorage, oAuthStorage } from '../storage/auth-storage';

export type AuthProvider = {
  get(): AbstractAuthFlow;
  provide(options: OAuth): void;
};

let _authProvider: AuthProvider | null = null;
export function getOrSetAuthProvider(
  factory?: () => AuthProvider,
): AuthProvider {
  if (_authProvider != null) {
    return _authProvider;
  }

  if (!factory) {
    throw new Error('AuthProvider has not been initialized.');
  }

  return (_authProvider = factory());
}

export abstract class AbstractAuthFlow {
  constructor(
    protected readonly options: OAuth,
    protected readonly storage: OAuthStorage = oAuthStorage,
  ) {}

  abstract initialize(): void | Promise<void>;
  abstract login(...args: unknown[]): Promise<void>;
  abstract logout(): Promise<void>;
  abstract redirectToLogin(
    navigator?: () => void,
    returnUrl?: string,
  ): void | Promise<void>;
}
