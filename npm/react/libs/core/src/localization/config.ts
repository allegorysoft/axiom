import type { Provider } from '../models/common';
import { provideInitializers } from '../initializer/registry';

import type { Translations } from './localization';
import { localizerStore, setCultureReloadHandler } from './localizer-store';
import {
  clientLocalizationProvider,
  remoteLocalizationProvider,
} from './localization-providers';
import { seed } from './localization-utils';

export type LocalizationOptions = {
  readonly defaultCulture?: string;
  readonly remote?: Partial<{
    skipProvider: boolean;
    /**
     * Default: `application/localization`
     */
    url: string;
  }>;

  readonly client?: Partial<{
    skipProvider: boolean;
    /**
     * Default: `i18n`
     */
    basePath: string;
  }>;

  /**
   * ```
   * const en = {
   *   provide() {
   *     return { AxiomAccount: { SignIn: 'Sign in (static)' } };
   *   },
   * };
   * ```
   */
  readonly translationProviders?: Record<string, Provider<Translations>>;
};

export function configureLocalization(options?: LocalizationOptions) {
  provideInitializers({
    configure: () =>
      seed(buildProviders(options?.defaultCulture, options), localizerStore),
  });

  setCultureReloadHandler((culture) =>
    seed(buildProviders(culture.name, options), localizerStore),
  );
}

function buildProviders(
  cultureName: string = 'en',
  options?: LocalizationOptions,
): Provider<Translations>[] {
  const providers: Provider<Translations>[] = [];

  if (!options?.remote?.skipProvider) {
    console.log(options?.remote?.url);
    providers.push(
      remoteLocalizationProvider({
        url: `/${options?.remote?.url ?? 'application-localization'}`,
        cultureName,
      }),
    );
  }

  if (!options?.client?.skipProvider) {
    providers.push(
      clientLocalizationProvider({
        fileNameOrPath: `/${options?.client?.basePath ?? 'i18n'}/${cultureName}`,
      }),
    );
  }

  const additionalProviders = options?.translationProviders?.[cultureName];
  if (additionalProviders) {
    providers.push(additionalProviders);
  }

  return providers;
}
