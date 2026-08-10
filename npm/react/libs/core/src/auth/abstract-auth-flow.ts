import type { OAuth } from '../models/oauth';

export type AuthProvider = (options: OAuth) => AbstractAuthFlow;

export abstract class AbstractAuthFlow {
  constructor(protected readonly options: OAuth) {}

  abstract login(...args: unknown[]): Promise<void>;
  abstract logout(): Promise<void>;
}
