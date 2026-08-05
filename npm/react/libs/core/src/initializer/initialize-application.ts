import type { AxiomApplicationOptions } from '../models/application';
import { isDevMode } from '../utils/is-dev-mode';
import { getInitializers } from './registry';
import { runInitializers } from './run';

export async function initializeApplication(
  options?: AxiomApplicationOptions,
): Promise<void> {
  if (isDevMode()) {
    console.info('[Axiom] Application initialization started.');
  }

  try {
    const initializers = [
      ...getInitializers(),
      ...(options?.initializers ?? []),
    ];

    if (initializers.length > 0) {
      await runInitializers(initializers);
    }

    if (isDevMode()) {
      console.info(`[Axiom] Application initialized successfully`);
    }
  } catch (error) {
    console.error('[Axiom] Application initialization failed.', error);
    throw error;
  }
}
