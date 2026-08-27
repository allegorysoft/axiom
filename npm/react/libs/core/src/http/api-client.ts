import { environmentStore } from '../environment/environment-store';
import { HttpRequest } from './http-client';
import { getHttpClient } from './http-client-factory';

interface ApiRequest extends HttpRequest {
  moduleName?: string;
}

class ApiClient {
  constructor(private readonly http = getHttpClient()) {}

  get<T>(path: string, request?: ApiRequest) {
    return this.http.get<T>(this.resolve(path, request?.moduleName), request);
  }

  post<T>(path: string, body?: unknown, request?: ApiRequest) {
    return this.http.post<T>(
      this.resolve(path, request?.moduleName),
      body,
      request,
    );
  }

  put<T>(path: string, body?: unknown, request?: ApiRequest) {
    return this.http.put<T>(
      this.resolve(path, request?.moduleName),
      body,
      request,
    );
  }

  patch<T>(path: string, body?: unknown, request?: ApiRequest) {
    return this.http.patch<T>(
      this.resolve(path, request?.moduleName),
      body,
      request,
    );
  }

  delete<T>(path: string, request?: ApiRequest) {
    return this.http.delete<T>(
      this.resolve(path, request?.moduleName),
      request,
    );
  }

  private resolve(path: string, moduleName?: string): string {
    const key = moduleName || 'Default';
    const state = environmentStore.get();
    const endpoint = state.environment?.endpoints[key];

    if (!endpoint) {
      throw new Error(`Module '${key}' is not configured in environment.`);
    }

    return `${endpoint.url}${path}`;
  }
}

let restClient: ApiClient | undefined;

export function getApiClient() {
  restClient ??= new ApiClient();

  return restClient;
}
