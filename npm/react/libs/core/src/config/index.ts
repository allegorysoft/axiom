import {
  type ErrorHandlerOptions,
  provideErrorHandler,
} from '../error-handling/error-handler';
import type { HttpClientOptions } from '../http/http-client-factory';
import { provideHttpClient } from '../http/provider';
import {
  type LocalizationOptions,
  configureLocalization,
} from '../localization/config';
import { configureNav, NavOptions } from '../nav/config';

type CoreOptions = {
  errorHandler?: ErrorHandlerOptions;
  httpClient?: HttpClientOptions;
  localization?: LocalizationOptions;
  nav?: NavOptions;
};

export function configureCore(options?: CoreOptions) {
  provideErrorHandler(options?.errorHandler);
  provideHttpClient(options?.httpClient);
  configureLocalization(options?.localization);
  configureNav();
}
