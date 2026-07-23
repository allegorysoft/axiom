export type Side = 'server' | 'client';

export interface InitializerContext {
  readonly side: Side;
  /** Aborted when this initializer's `timeout` elapses. */
  readonly signal: AbortSignal;
}

export interface Initializer {
  /** Unique id. Re-providing the same name replaces the previous one. */
  name: string;
  run: (context: InitializerContext) => void | Promise<void>;
  /** Where it may run. Default: 'both'. */
  side?: Side | 'both';
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
