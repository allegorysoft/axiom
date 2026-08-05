import type { Provider } from '../models/common';
import type { Translations, LocalizerStore } from '../models/localization';

export async function seed(
  providers: Provider<Translations>[],
  store: LocalizerStore,
): Promise<void> {
  store.setStatus('loading');

  const failures: unknown[] = [];
  let succeeded = false;

  for (const { provide } of providers) {
    try {
      const translations = await provide();

      store.setTranslations(translations);
      succeeded = true;
    } catch (error) {
      failures.push(error);
      console.error(
        '[Axiom-localization-utils] provider failed to seed:',
        error,
      );
    }
  }

  store.setStatus(
    succeeded ? 'ready' : 'error',
    succeeded ? undefined : failures,
  );
}
