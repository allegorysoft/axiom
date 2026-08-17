import type { OAuth } from '../models/oauth';
import { type OAuthStorage, oAuthStorage } from '../storage/auth-storage';

export type AuthProvider = {
  get(): AbstractAuthFlow;
  provide(options: OAuth): void;
};

export abstract class AbstractAuthFlow {
  constructor(
    protected readonly options: OAuth,
    protected readonly storage: OAuthStorage = oAuthStorage,
  ) {}

  abstract initialize(): void | Promise<void>;
  abstract login(...args: unknown[]): Promise<void>;
  abstract logout(): Promise<void>;
}
