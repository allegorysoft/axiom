import type { Provider } from '../models/common';
import type { Translations } from '../models/localization';
import { HttpClient } from '../http/http-client';
import { getApiClient } from '../http/api-client';

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
      const client = getApiClient();
      const query = { culture: options.cultureName };
      return client
        .get<Response[]>(options.url, { query })
        .then((response) => response[0].translations);
    },
  };
}

type JsonFileProviderOptions = {
  readonly fileNameOrPath: string;
};

function jsonFileLocalizationProvider(
  options: JsonFileProviderOptions,
): Provider<Translations> {
  return {
    provide() {
      const client = new HttpClient();
      return client.get<Translations>(`${options.fileNameOrPath}.json`, {
        headers: { Accept: 'application/json' },
      });
    },
  };
}

export { remoteLocalizationProvider, jsonFileLocalizationProvider };
