import type { Provider } from '../models/common';
import type { Translations } from '../models/localization';
import { provideInitializers } from '../initializer/registry';
import type { HttpMiddleware } from '../http/http-client';
import { createHttpClient } from '../http/http-client-factory';
import {
  remoteLocalizationProvider,
  clientLocalizationProvider,
} from '../providers/localization-providers';
import {
  setCultureReloadHandler,
  localizerStore,
} from '../store/localizer-store';
import { seed } from '../utils/localization-utils';

type LocalizationOptions = {
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

  if (options?.providers?.length) {
    providers.push(...options.providers);
  }

  provideInitializers({ configure: () => seed(providers, localizerStore) });

  setCultureReloadHandler(async (culture) => {
    await seed(
      [remoteProvider(culture.name), clientLocalizer(culture.name)],
      localizerStore,
    );
  });
}

type HttpClientOptions = { middlewares?: HttpMiddleware[] };

export function provideHttpClient(options?: HttpClientOptions) {
  const middlewares = options?.middlewares ?? [];
  createHttpClient({ middlewares });
}

type CoreOptions = {
  localization?: LocalizationOptions;
  httpClient?: HttpClientOptions;
};

export function configureCore(options?: CoreOptions) {
  provideHttpClient(options?.httpClient);
  configureLocalization(options?.localization);
}
