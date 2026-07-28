import type { AxiomApplicationOptions } from '../models/application';
import { getInitializers } from './registry';
import { runInitializers } from './run';

export async function bootstrapApplication(
  options?: AxiomApplicationOptions,
): Promise<void> {
  const isDev = import.meta.env.DEV;
  if (isDev) {
    console.info('[Axiom] Application initialization started.');
  }

  try {
    const initializers = [
      ...getInitializers(),
      ...(options?.initializers ?? []),
    ];

    await runInitializers(initializers);

    if (isDev) {
      console.info(`[Axiom] Application initialized successfully`);
    }
  } catch (error) {
    console.error('[Axiom] Application initialization failed.', error);
    throw error;
  }
}
