import { createStoreHook } from '../store/axiom-store';
import { localizerStore } from '../store/localizer-store';

export const useLocalizer = createStoreHook(localizerStore);

/**
 * Reactive translation API for React
 * @param moduleName
 * @returns
 */
export function useTranslation(moduleName?: string) {
  useLocalizer((s) => s.culture.name);
  useLocalizer((s) => s.translations);

  return (
    key: string,
    args?: Record<string, unknown> | readonly unknown[],
  ): string => localizerStore.localize(key, moduleName, args);
}
