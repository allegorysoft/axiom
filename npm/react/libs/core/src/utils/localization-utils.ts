import type { Provider } from '../models/common';
import type { Translations, LocalizerStore } from '../models/localization';

export async function seed(
  providers: Provider<Translations>[],
  store: LocalizerStore,
): Promise<void> {
  store.setStatus('loading');

  const results = await Promise.allSettled(
    providers.map(({ provide }) => provide()),
  );

  const failures: unknown[] = [];

  for (const result of results) {
    if (result.status === 'fulfilled') {
      store.setTranslations(result.value);
      continue;
    }

    failures.push(result.reason);
    console.error('[Axiom-localizer] provider failed to seed:', result.reason);
  }

  if (failures.length === results.length && results.length > 0) {
    store.setStatus('error', failures);
  } else {
    store.setStatus('ready');
  }
}
