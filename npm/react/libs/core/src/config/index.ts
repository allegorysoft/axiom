import type { Provider } from '../models/common';
import type { CultureInfo, Translations } from '../models/localization';
import { provideInitializers } from '../initializer/registry';
import type { HttpMiddleware } from '../http/http-client';
import { createHttpClient } from '../http/http-client-factory';
import {
  remoteLocalizationProvider,
  jsonFileLocalizationProvider,
} from '../providers/localization-providers';
import {
  setCultureReloadHandler,
  localizerStore,
} from '../store/localizer-store';
import { seed } from '../utils/localization-utils';

type LocalizationOptions = {
  readonly providers?: Provider<Translations>[];
  readonly defaultCulture?: string;
};

export function configureLocalization(options?: LocalizationOptions) {
  const remoteProvider = (cultureName: string) =>
    remoteLocalizationProvider({
      url: '/application-localization', //Will be /application/localization?culture=<value>
      cultureName,
    });

  const jsonLocalizer = (cultureName: string) =>
    jsonFileLocalizationProvider({
      fileNameOrPath: `/i18n/${cultureName}`,
    });

  const providers = [
    remoteProvider(options?.defaultCulture || 'en'),
    jsonLocalizer(options?.defaultCulture || 'en'),
    ...(options?.providers ?? []),
  ];

  provideInitializers({ configure: () => seed(providers, localizerStore) });

  //TODO: Improve this behavior ?
  let reloadId = 0;
  setCultureReloadHandler(async (culture) => {
    const current = ++reloadId;
    await seed(
      [remoteProvider(culture.name), jsonLocalizer(culture.name)],
      localizerStore,
    );

    if (current !== reloadId) {
      return;
    }
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
