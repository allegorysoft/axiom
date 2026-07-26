import type { Platform } from '../models/application';

export interface InitializerContext {
  readonly platform: Platform;
  /** Aborted when this initializer's `timeout` elapses. */
  readonly signal: AbortSignal;
}

export type ConfigureFn = (context: InitializerContext) => void | Promise<void>;

export interface ApplicationInitializer {
  /** Unique name. Re-providing the same name replaces the previous one. */
  name: string;
  configure: ConfigureFn;
  postConfigure?: ConfigureFn;
  /** Where it may run. Default: 'both'. */
  platform?: Platform | 'both';
  /** Failure is logged instead of aborting the boot. */
  optional?: boolean;
  /** Timeout in milliseconds. */
  timeout?: number;
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
