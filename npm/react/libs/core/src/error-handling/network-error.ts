import { NetworkError, type NetworkErrorContext } from './models';

export function normalizeNetworkError(
  error: unknown,
  context: NetworkErrorContext = {},
): NetworkError | undefined {
  if (error instanceof NetworkError) {
    return error;
  }

  if (isAbortError(error)) {
    return new NetworkError('aborted', context, { cause: error });
  }

  // fetch() network failures are normally surfaced as TypeError.
  if (error instanceof TypeError) {
    const reason =
      typeof navigator !== 'undefined' && !navigator.onLine
        ? 'offline'
        : 'unreachable';

    return new NetworkError(reason, context, { cause: error });
  }

  return undefined;
}

function isAbortError(error: unknown): error is DOMException {
  return error instanceof DOMException && error.name === 'AbortError';
}
