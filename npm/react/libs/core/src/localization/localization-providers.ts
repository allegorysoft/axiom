import type { Provider } from '../models/common';
import type { Translations } from './localization';
import { getHttpClient } from '../http/http-client-factory';
import { getOrCreateApiClient } from '../http/api-client';

type RemoteProviderOptions = {
  readonly url: string;
  readonly cultureName?: string;
  readonly headers?: Record<string, string>;
};
type Response = { translations: Translations };

function remoteLocalizationProvider(
  options: RemoteProviderOptions,
): Provider<Translations> {
  return {
    provide() {
      //TODO: Update response return object with CultureInfo
      const client = getOrCreateApiClient();
      const query = { culture: options.cultureName };
      return client
        .get<Response[]>(options.url, { query })
        .then((response) => response[0].translations);
    },
  };
}

type ClientProviderOptions = {
  readonly fileNameOrPath: string;
};

function clientLocalizationProvider(
  options: ClientProviderOptions,
): Provider<Translations> {
  return {
    provide() {
      const client = getHttpClient();
      return client.get<Translations>(`${options.fileNameOrPath}.json`, {
        headers: { Accept: 'application/json' },
      });
    },
  };
}

export { remoteLocalizationProvider, clientLocalizationProvider };
