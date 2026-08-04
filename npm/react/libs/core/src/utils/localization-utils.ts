import type { Provider } from '../models/common';
import type { Translations, LocalizerStore } from '../models/localization';

export async function seed(
  providers: Provider<Translations>[],
  store: LocalizerStore,
): Promise<void> {
  store.setStatus('loading');

  const failures: unknown[] = [];

  for (const { provide } of providers) {
    try {
      const translations = await provide();

      store.setTranslations(translations);
    } catch (error) {
      failures.push(error);
      console.error(
        '[Axiom-localization-utils] provider failed to seed:',
        error,
      );
    }
  }

  if (failures.length === providers.length && providers.length > 0) {
    store.setStatus('error', failures);
  } else {
    store.setStatus('ready');
  }
}
