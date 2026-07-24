import { getInitializers } from './registry';
import { runInitializers } from './run';
import type { Initializer } from '../models/initializer';
import type { Platform } from '../utils/platform-utils';

let inflight: Promise<void> | null = null;

export function bootstrapApplication(
  options: { initializers?: Initializer[]; platform?: Platform } = {},
): Promise<void> {
  inflight ??= runInitializers(
    options.initializers ?? getInitializers(),
    options.platform,
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
