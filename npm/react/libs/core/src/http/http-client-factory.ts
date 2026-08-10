import { HttpClient, type HttpMiddleware } from './http-client';

const DEFAULT_KEY = 'default';
const clients = new Map<string, HttpClient>();

export interface HttpClientOptions {
  middlewares?: HttpMiddleware[];
}

export function createHttpClient(
  options: HttpClientOptions = {},
  name = DEFAULT_KEY,
): HttpClient {
  const existing = clients.get(name);

  if (existing) {
    if (name === DEFAULT_KEY) {
      return existing;
    }

    throw new Error(`HttpClient "${name}" already created.`);
  }

  const client = new HttpClient();

  for (const middleware of options.middlewares ?? []) {
    client.use(middleware);
  }

  clients.set(name, client);

  return client;
}

export function getHttpClient(name = DEFAULT_KEY): HttpClient {
  const client = clients.get(name);
  if (!client) {
    throw new Error(`HttpClient "${name}" not created.`);
  }

  return client;
}

export function removeHttpClient(name: string): void {
  clients.delete(name);
}
