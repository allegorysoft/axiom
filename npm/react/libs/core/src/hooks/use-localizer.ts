import { createStoreHook } from '../store/axiom-store';
import { localizerStore } from '../store/localizer-store';

export const useLocalizer = createStoreHook(localizerStore);

/**
 * Automatically updates when the active culture or translations change.
 *
 * @param moduleName Translation module to resolve keys from. Defaults to "Default".
 * @returns Translation function for looking up localized strings.
 */
export function useTranslation(moduleName?: string) {
  useLocalizer((s) => s.culture.name);
  useLocalizer((s) => s.translations);

  return (
    key: string,
    args?: Record<string, unknown> | readonly unknown[],
  ): string => localizerStore.localize(key, moduleName, args);
}
