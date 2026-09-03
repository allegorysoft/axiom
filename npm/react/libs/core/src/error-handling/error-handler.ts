import { HttpError, type NetworkError } from './models';
import { HttpStatusErrorHandler } from './http-status-error-handler';
import { normalizeNetworkError } from './network-error';

type NetworkErrorHandler = (error: NetworkError) => void;
type UnknownErrorHandler = (error: unknown) => void;

export interface ErrorHandlerOptions {
  handlers?: Partial<Record<number, (error: HttpError) => void>>;
  onNetworkError?: NetworkErrorHandler;
  onUnknownError?: UnknownErrorHandler;
}

const networkErrorHandler: NetworkErrorHandler = (error) =>
  console.error(`[${error.reason}]`, error.message, error.cause ?? '');

const unknownErrorHandler: UnknownErrorHandler = (error) =>
  console.error(error);

export class ErrorHandler {
  protected readonly statusHandlers: HttpStatusErrorHandler;
  protected onNetworkError: NetworkErrorHandler;
  protected onUnknownError: UnknownErrorHandler;

  constructor(options: ErrorHandlerOptions = {}) {
    this.statusHandlers = new HttpStatusErrorHandler(options.handlers);
    this.onNetworkError = options.onNetworkError ?? networkErrorHandler;
    this.onUnknownError = options.onUnknownError ?? unknownErrorHandler;
  }

  set(options: ErrorHandlerOptions): void {
    if (options.handlers) {
      this.statusHandlers.set(options.handlers);
    }

    if (options.onNetworkError) {
      this.onNetworkError = options.onNetworkError;
    }

    if (options.onUnknownError) {
      this.onUnknownError = options.onUnknownError;
    }
  }

  handle(error: unknown, context?: { url: string; method: string }): void {
    const networkError = normalizeNetworkError(error, context);
    if (networkError) {
      this.onNetworkError(networkError);
      return;
    }

    if (error instanceof HttpError && this.statusHandlers.handle(error)) {
      return;
    }

    this.onUnknownError(error);
  }
}

let errorHandler: ErrorHandler;
export function provideErrorHandler(
  options?: ErrorHandlerOptions,
): ErrorHandler {
  return (errorHandler = new ErrorHandler(options));
}

export function getErrorHandler(): ErrorHandler {
  if (errorHandler === undefined || errorHandler === null) {
    throw new Error('ErrorHandler not provided');
  }

  return errorHandler;
}
