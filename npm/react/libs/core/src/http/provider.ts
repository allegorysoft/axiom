import { exceptionHandlerMiddleware } from '../middlewares/exception-handler-middleware';
import {
  type HttpClientOptions,
  createHttpClient,
} from './http-client-factory';

export function provideHttpClient(options?: HttpClientOptions) {
  createHttpClient({
    middlewares: [exceptionHandlerMiddleware, ...(options?.middlewares ?? [])],
  });
}
