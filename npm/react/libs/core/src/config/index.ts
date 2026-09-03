import {
  type ErrorHandlerOptions,
  provideErrorHandler,
} from '../error-handling/error-handler';
import type { HttpClientOptions } from '../http/http-client-factory';
import { provideHttpClient } from '../http/providers';
import {
  type LocalizationOptions,
  configureLocalization,
} from '../localization/config';

type CoreOptions = {
  errorHandler?: ErrorHandlerOptions;
  httpClient?: HttpClientOptions;
  localization?: LocalizationOptions;
};

export function configureCore(options?: CoreOptions) {
  provideErrorHandler(options?.errorHandler);
  provideHttpClient(options?.httpClient);
  configureLocalization(options?.localization);
}
