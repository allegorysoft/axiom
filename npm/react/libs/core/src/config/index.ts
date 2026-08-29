import type { HttpClientOptions } from '../http/http-client-factory';
import { provideHttpClient } from '../http/providers';
import {
  type LocalizationOptions,
  configureLocalization,
} from '../localization/config';

type CoreOptions = {
  httpClient?: HttpClientOptions;
  localization?: LocalizationOptions;
};

export function configureCore(options?: CoreOptions) {
  provideHttpClient(options?.httpClient);
  configureLocalization(options?.localization);
}
