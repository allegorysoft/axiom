import type { Platform } from '../utils/platform-utils';

export interface InitializerContext {
  readonly platform: Platform;
  /** Aborted when this initializer's `timeout` elapses. */
  readonly signal: AbortSignal;
}

export interface Initializer {
  /** Unique id. Re-providing the same name replaces the previous one. */
  name: string;
  configure: (context: InitializerContext) => void | Promise<void>;
  postConfigure?: (context: InitializerContext) => void | Promise<void>;
  /** Where it may run. Default: 'both'. */
  platform?: Platform | 'both';
  /** Failure is logged instead of aborting the boot. */
  optional?: boolean;
  /** Milliseconds. */
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
