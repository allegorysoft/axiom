import type { Provider } from '../models/common';
import type { Translations } from '../models/localization';
import { provideInitializers } from '../initializer/registry';
import type { HttpMiddleware } from '../http/http-client';
import { createHttpClient } from '../http/http-client-factory';
import {
  remoteLocalizationProvider,
  jsonFileLocalizationProvider,
} from '../providers/localization-providers';
import { localizerStore } from '../store/localizer-store';
import { seed } from '../utils/localization-utils';

type LocalizationOptions = {
  readonly providers?: Provider<Translations>[];
  readonly defaultCulture?: string;
};

export function configureLocalization(options?: LocalizationOptions) {
  const providers = [
    remoteLocalizationProvider({
      url: '/application-localization',
      cultureName: 'en',
    }),
    jsonFileLocalizationProvider({
      fileNameOrPath: '/i18n/en',
    }),
    ...(options?.providers ?? []),
  ];

  provideInitializers({ configure: () => seed(providers, localizerStore) });
}

type HttpClientOptions = { middlewares?: HttpMiddleware[] };

export function provideHttpClient(options?: HttpClientOptions) {
  const middlewares = options?.middlewares ?? [];
  createHttpClient({ middlewares });
}
