import type { Platform } from '../models/application';

type Context = { platform: Platform };
export type ConfigureFn = (context?: Context) => void | Promise<void>;

export interface ApplicationInitializer {
  configure?: ConfigureFn;
  postConfigure?: ConfigureFn;
  /** Where it may run. Default: 'client'. */
  platform?: Platform | 'both';
}

export class InitializerError extends Error {
  readonly initializer: string;

  constructor(initializer: string, cause: unknown) {
    const reason = cause instanceof Error ? cause.message : String(cause);
    super(`Initializer "${initializer}" failed: ${reason}`);
    this.name = 'InitializerError';
    this.initializer = initializer;
  }
}
