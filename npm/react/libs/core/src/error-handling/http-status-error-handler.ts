import type { HttpError } from './models';

type StatusHandlers = Partial<Record<number, (error: HttpError) => void>>;

const DEFAULTS: StatusHandlers = {
  400: (error) => console.warn(error),
  401: (error) => console.warn(error),
  403: (error) => console.warn(error),
  404: (error) => console.warn(error),
  500: (error) => console.error(error),
};

export class HttpStatusErrorHandler {
  protected readonly handlers: StatusHandlers;

  constructor(handlers?: StatusHandlers) {
    this.handlers = {
      ...DEFAULTS,
      ...handlers,
    };
  }

  set(handlers: StatusHandlers): void {
    for (const status in handlers) {
      this.handlers[status] = handlers[status];
    }
  }

  handle(error: HttpError): boolean {
    const handler = this.handlers[error.status];

    if (!handler) {
      return false;
    }

    handler(error);
    return true;
  }
}
