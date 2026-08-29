import {
  type HttpClientOptions,
  createHttpClient,
} from './http-client-factory';

export function provideHttpClient(options?: HttpClientOptions) {
  const middlewares = options?.middlewares ?? [];
  createHttpClient({ middlewares });
}
