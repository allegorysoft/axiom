import type {
  LocalizerState,
  CultureInfo,
  Translations,
  LocalizerStore,
  LocalizerStatus,
} from '../models/localization';
import { createStore } from './axiom-store';

const initialState: LocalizerState = {
  translations: {},
  culture: {
    name: 'en',
    displayName: 'English',
  },
  status: 'idle',
  error: null,
};

const baseStore = createStore<LocalizerState>(initialState);

type CultureReloadHandler = (culture: CultureInfo) => void | Promise<void>;
let reloadHandler: CultureReloadHandler | undefined;

export function setCultureReloadHandler(handler: CultureReloadHandler) {
  reloadHandler = handler;
}

export const localizerStore: LocalizerStore = Object.assign(baseStore, {
  /**
   * Imperative translation API
   * @param key
   * @param moduleName
   * @param args
   * @returns
   */
  localize(
    key: string,
    moduleName?: string,
    args: Record<string, unknown> | readonly unknown[] = [],
  ): string {
    const module = baseStore.get().translations[moduleName ?? 'Default'];
    const value = module?.[key] ?? key;

    if (Array.isArray(args)) {
      return args.length > 0 ? format(value, args) : value;
    }

    return Object.keys(args).length > 0 ? format(value, args) : value;
  },

  setTranslations(incoming: Translations, overwrite = true): void {
    baseStore.set((prev) => {
      let changed = false;
      const next: Translations = structuredClone(prev.translations);

      for (const [moduleName, texts] of Object.entries(incoming)) {
        const module = (next[moduleName] ??= {});

        for (const [key, value] of Object.entries(texts)) {
          const hasNoValue = module[key] === undefined;
          if ((overwrite || hasNoValue) && module[key] !== value) {
            module[key] = value;
            changed = true;
          }
        }
      }

      return changed ? { translations: next } : {};
    });
  },

  setCulture(culture: CultureInfo): void {
    let changed = false;
    baseStore.set((prev) => {
      changed = !Object.entries(culture).every(
        ([key, value]) => prev.culture[key as keyof CultureInfo] === value,
      );

      return changed ? { culture } : {};
    });

    if (changed) {
      void reloadHandler?.(culture);
    }
  },

  setStatus(status: LocalizerStatus, error: unknown = null): void {
    baseStore.set(() => ({
      status,
      error,
    }));
  },

  reset(): void {
    baseStore.set(() => structuredClone(initialState));
  },
});

function format(
  template: string,
  args: Record<string, unknown> | readonly unknown[],
): string {
  return template.replace(/\{([^}]+)\}/g, (_, key: string) => {
    if (Array.isArray(args)) {
      return String(args[Number(key)] ?? '');
    }

    return String((args as Record<string, unknown>)[key] ?? '');
  });
}
