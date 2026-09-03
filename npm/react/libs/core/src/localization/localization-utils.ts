import type { Provider } from '../models/common';
import type { Translations, LocalizerStore } from './localization';

export async function seed(
  providers: Provider<Translations>[],
  store: LocalizerStore,
): Promise<void> {
  let merged: Translations = {};

  const results = await Promise.allSettled(providers.map((p) => p.provide()));
  for (const result of results.filter((f) => f.status === 'fulfilled')) {
    merged = merge(merged, result.value);
  }

  store.setTranslations(merged);
}

function merge(target: Translations, source: Translations): Translations {
  const result: Translations = { ...target };

  for (const [moduleName, texts] of Object.entries(source)) {
    result[moduleName] = { ...result[moduleName], ...texts };
  }

  return result;
}
