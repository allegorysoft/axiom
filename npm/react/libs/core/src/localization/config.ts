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
  readonly providers?: Provider<Translations>[];
  readonly defaultCulture?: string;
  readonly skipProvider?: Partial<{ remote: boolean; client: boolean }>;
};

export function configureLocalization(options?: LocalizationOptions) {
  const remoteProvider = (cultureName: string) =>
    remoteLocalizationProvider({
      url: '/application-localization', //Will be /application/localization?culture=<value>
      cultureName,
    });

  const clientLocalizer = (cultureName: string) =>
    clientLocalizationProvider({
      fileNameOrPath: `/i18n/${cultureName}`,
    });

  const providers: Provider<Translations>[] = [];
  if (!options?.skipProvider?.remote) {
    providers.push(remoteProvider(options?.defaultCulture || 'en'));
  }

  if (!options?.skipProvider?.client) {
    providers.push(clientLocalizer(options?.defaultCulture || 'en'));
  }

  //TODO: Invalid approach
  if (options?.providers?.length) {
    providers.push(...options.providers);
  }

  provideInitializers({ configure: () => seed(providers, localizerStore) });

  setCultureReloadHandler(async (culture) => {
    const providers: Provider<Translations>[] = [];
    if (!options?.skipProvider?.remote) {
      providers.push(remoteProvider(culture.name));
    }

    if (!options?.skipProvider?.client) {
      providers.push(clientLocalizer(culture.name));
    }

    //TODO: Invalid approach
    if (options?.providers?.length) {
      providers.push(...options.providers);
    }

    await seed([...providers], localizerStore);
  });
}
