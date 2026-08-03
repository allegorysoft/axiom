import type { Provider } from '../models/common';
import type { Translations } from '../models/localization';
import { provideInitializers } from '../initializer/registry';
import { localizerStore } from '../store/localizer-store';
import { seed } from '../utils/localization-utils';

async function fetchJson<T>(
  url: string | URL,
  headers?: Record<string, string>,
): Promise<T> {
  const response = await fetch(url, { headers });

  if (!response.ok) {
    throw new Error(
      `request to "${url}" failed: ${response.status} ${response.statusText}`,
    );
  }

  return response.json();
}

type RemoteProviderOptions = {
  readonly url: string;
  readonly cultureName?: string;
  readonly headers?: Record<string, string>;
};

function remoteLocalizationProvider(
  options: RemoteProviderOptions,
): Provider<Translations> {
  return {
    provide() {
      const url = new URL(options.url);
      if (options.cultureName) {
        url.searchParams.set('culture', options.cultureName);
      }

      return fetchJson<Translations>(url, options.headers);
    },
  };
}

type JsonFileProviderOptions = { readonly fileNameOrPath: string };

function jsonFileLocalizationProvider(
  options: JsonFileProviderOptions,
): Provider<Translations> {
  return {
    provide() {
      return fetchJson<Translations>(`${options.fileNameOrPath}.json`, {
        Accept: 'application/json',
      });
    },
  };
}

type LocalizationOptions = {
  readonly providers?: Provider<Translations>[];
};

function provideLocalization(options?: LocalizationOptions) {
  provideInitializers({
    configure: async () => {
      await seed(
        [
          jsonFileLocalizationProvider({ fileNameOrPath: 'i18n/en' }),
          ...(options?.providers ?? []),
        ],
        localizerStore,
      );
    },
  });
}

export {
  remoteLocalizationProvider,
  jsonFileLocalizationProvider,
  provideLocalization,
};
