export class HttpError extends Error {
  constructor(
    public readonly response: Response,
    options?: ErrorOptions,
  ) {
    super(`${response.status} ${response.statusText}`, options);
    this.name = 'HttpError';
  }

  get status(): number {
    return this.response.status;
  }

  get statusText(): string {
    return this.response.statusText;
  }
}

type NetworkErrorReason = 'offline' | 'timeout' | 'aborted' | 'unreachable';

export interface NetworkErrorContext {
  readonly url?: string;
  readonly method?: string;
}

export class NetworkError extends Error {
  readonly reason: NetworkErrorReason;
  readonly url?: string;
  readonly method?: string;

  constructor(
    reason: NetworkErrorReason,
    context: NetworkErrorContext = {},
    options?: ErrorOptions,
  ) {
    super(NetworkError.messageFor(reason, context), options);

    this.name = 'NetworkError';
    this.reason = reason;
    this.url = context.url;
    this.method = context.method;
  }

  private static messageFor(
    reason: NetworkErrorReason,
    context: NetworkErrorContext,
  ): string {
    const target = context.url
      ? ` (${context.method ?? 'GET'} ${context.url})`
      : '';

    switch (reason) {
      case 'offline':
        return `Network request failed: browser is offline${target}`;
      case 'timeout':
        return `Network request timed out${target}`;
      case 'aborted':
        return `Network request was aborted${target}`;
      case 'unreachable':
        return `Network request could not reach the server${target}`;
    }
  }
}
