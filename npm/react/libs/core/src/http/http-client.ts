export type QueryParams = Record<
  string,
  string | number | boolean | null | undefined
>;

export interface HttpRequest extends RequestInit {
  query?: QueryParams;
}

export interface HttpContext {
  url: string;
  request: HttpRequest;
}

export type HttpNext = () => Promise<Response>;

export type HttpMiddleware = (
  context: HttpContext,
  next: HttpNext,
) => Promise<Response>;

export class HttpClient {
  private readonly middlewares: HttpMiddleware[] = [];

  constructor(
    private readonly fetcher: typeof fetch = (...args) => fetch(...args),
  ) {}

  use(middleware: HttpMiddleware): this {
    this.middlewares.push(middleware);
    return this;
  }

  get<T>(url: string, request?: HttpRequest): Promise<T> {
    return this.request('GET', url, undefined, request);
  }

  post<T>(url: string, body?: unknown, request?: HttpRequest): Promise<T> {
    return this.request('POST', url, body, request);
  }

  put<T>(url: string, body?: unknown, request?: HttpRequest): Promise<T> {
    return this.request('PUT', url, body, request);
  }

  patch<T>(url: string, body?: unknown, request?: HttpRequest): Promise<T> {
    return this.request('PATCH', url, body, request);
  }

  delete<T>(url: string, request?: HttpRequest): Promise<T> {
    return this.request('DELETE', url, undefined, request);
  }

  private async request<T>(
    method: string,
    url: string,
    body?: unknown,
    request: HttpRequest = {},
  ): Promise<T> {
    const context: HttpContext = {
      url,
      request: {
        ...request,
        method,
      },
    };

    if (body !== undefined) {
      context.request.body = JSON.stringify(body);
      context.request.headers = {
        'Content-Type': 'application/json',
        ...context.request.headers,
      };
    }

    const response = await this.dispatch(context, 0);

    if (!response.ok) {
      throw new Error(`${response.status} ${response.statusText}`);
    }

    if (response.status === 204) {
      return undefined as T;
    }

    return response.json() as Promise<T>;
  }

  private dispatch(context: HttpContext, index: number): Promise<Response> {
    if (index === this.middlewares.length) {
      const { query, ...init } = context.request;
      return this.fetcher(this.buildUrl(context.url, query), init);
    }

    return this.middlewares[index](context, () =>
      this.dispatch(context, index + 1),
    );
  }

  private buildUrl(url: string, query?: QueryParams): string {
    if (!query) {
      return url;
    }

    const params = new URLSearchParams();

    for (const [key, value] of Object.entries(query)) {
      if (value != null) {
        params.set(key, String(value));
      }
    }

    const queryString = params.toString();

    if (!queryString) {
      return url;
    }

    return `${url}${url.includes('?') ? '&' : '?'}${queryString}`;
  }
}
