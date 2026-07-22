import { getInitializers } from './registry';
import { runInitializers } from './run';
import type { Initializer, Side } from './types';

let inflight: Promise<void> | null = null;

export function bootstrapApplication(
  options: { initializers?: Initializer[]; side?: Side } = {},
): Promise<void> {
  inflight ??= runInitializers(
    options.initializers ?? getInitializers(),
    options.side,
  )
    .then(() => undefined)
    .catch((error) => {
      inflight = null;
      throw error;
    });

  return inflight;
}

export function resetBootstrap(): void {
  inflight = null;
}
