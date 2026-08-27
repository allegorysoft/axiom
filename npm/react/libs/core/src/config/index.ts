import type { HttpMiddleware } from '../http/http-client';
import { createHttpClient } from '../http/http-client-factory';
import {
  type LocalizationOptions,
  configureLocalization,
} from '../localization/config';

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
