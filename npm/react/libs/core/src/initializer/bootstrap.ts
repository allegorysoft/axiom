import { getPlatform } from 'src/utils';
import type { AxiomApplicationOptions, Platform } from '../models/application';
import { getInitializers } from './registry';
import { runInitializers } from './run';

export interface BootstrapContext {
  platform: Platform;
}

export async function bootstrapApplication(
  context?: BootstrapContext,
  options?: AxiomApplicationOptions,
): Promise<void> {
  context ??= { platform: getPlatform() };

  const isDev = import.meta.env.DEV;
  if (isDev) {
    console.info('[Axiom] Application initialization started.');
  }

  try {
    const initializers = [
      ...getInitializers(),
      ...(options?.initializers ?? []),
    ];

    await runInitializers(initializers, { platform: context.platform });

    if (isDev) {
      console.info(`[Axiom] Application initialized successfully`);
    }
  } catch (error) {
    console.error('[Axiom] Application initialization failed.', error);
    throw error;
  }
}
