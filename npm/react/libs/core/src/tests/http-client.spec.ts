import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { HttpClient } from '../http/http-client';

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  });
}

describe('HttpClient', () => {
  const fetchMock = vi.fn<typeof fetch>();

  beforeEach(() => {
    fetchMock.mockReset();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('basic requests', () => {
    it('sends a GET request', async () => {
      fetchMock.mockResolvedValue(jsonResponse({ id: 1 }));

      const client = new HttpClient(fetchMock);
      const result = await client.get<{ id: number }>('/users');

      expect(result).toEqual({ id: 1 });
      expect(fetchMock).toHaveBeenCalledWith('/users', { method: 'GET' });
    });

    it('sends a DELETE request', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.delete('/users/1');

      expect(fetchMock).toHaveBeenCalledWith('/users/1', { method: 'DELETE' });
    });

    it.each([
      ['post', 'POST'],
      ['put', 'PUT'],
      ['patch', 'PATCH'],
    ] as const)(
      'serializes the request body as JSON for %s',
      async (method, verb) => {
        fetchMock.mockResolvedValue(jsonResponse({}));

        const client = new HttpClient(fetchMock);
        await client[method]('/users', { name: 'John' });

        expect(fetchMock).toHaveBeenCalledWith('/users', {
          method: verb,
          body: JSON.stringify({ name: 'John' }),
          headers: {
            'Content-Type': 'application/json',
          },
        });
      },
    );

    it('does not set a body or Content-Type when no body is provided', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.post('/users');

      expect(fetchMock).toHaveBeenCalledWith('/users', { method: 'POST' });
    });

    it('preserves caller-provided headers alongside Content-Type', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.post(
        '/users',
        { name: 'John' },
        {
          headers: { Authorization: 'Bearer token' },
        },
      );

      expect(fetchMock).toHaveBeenCalledWith('/users', {
        method: 'POST',
        body: JSON.stringify({ name: 'John' }),
        headers: {
          'Content-Type': 'application/json',
          Authorization: 'Bearer token',
        },
      });
    });

    it('lets caller-provided Content-Type override the default', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.post(
        '/users',
        { name: 'John' },
        {
          headers: { 'Content-Type': 'application/merge-patch+json' },
        },
      );

      expect(fetchMock).toHaveBeenCalledWith('/users', {
        method: 'POST',
        body: JSON.stringify({ name: 'John' }),
        headers: {
          'Content-Type': 'application/merge-patch+json',
        },
      });
    });

    it('defaults to the global fetch when no fetcher is provided', async () => {
      const globalFetchSpy = vi
        .spyOn(globalThis, 'fetch')
        .mockResolvedValue(jsonResponse({ ok: true }));

      const client = new HttpClient();
      const result = await client.get<{ ok: boolean }>('/users');

      expect(result).toEqual({ ok: true });
      expect(globalFetchSpy).toHaveBeenCalledWith('/users', { method: 'GET' });
    });
  });

  describe('query parameters', () => {
    it('appends query parameters', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.get('/users', { query: { page: 1, active: true } });

      expect(fetchMock).toHaveBeenCalledWith(
        '/users?page=1&active=true',
        expect.any(Object),
      );
    });

    it('does not append a "?" when query is empty', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.get('/users', { query: {} });

      expect(fetchMock).toHaveBeenCalledWith('/users', expect.any(Object));
    });

    it('omits null and undefined query values', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.get('/users', {
        query: { page: 1, filter: null, sort: undefined },
      });

      expect(fetchMock).toHaveBeenCalledWith(
        '/users?page=1',
        expect.any(Object),
      );
    });

    it('appends with "&" when the url already has a query string', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.get('/users?sort=asc', { query: { page: 1 } });

      expect(fetchMock).toHaveBeenCalledWith(
        '/users?sort=asc&page=1',
        expect.any(Object),
      );
    });

    it('does not leak the "query" option into the fetch RequestInit', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);
      await client.get('/users', { query: { page: 1 } });

      const [, init] = fetchMock.mock.calls[0]!;
      expect(init).not.toHaveProperty('query');
    });
  });

  describe('middleware', () => {
    it('executes middleware in registration order', async () => {
      const calls: string[] = [];

      fetchMock.mockImplementation(async () => {
        calls.push('fetch');
        return jsonResponse({});
      });

      const client = new HttpClient(fetchMock);

      client.use(async (_, next) => {
        calls.push('first-before');
        const response = await next();
        calls.push('first-after');
        return response;
      });

      client.use(async (_, next) => {
        calls.push('second-before');
        const response = await next();
        calls.push('second-after');
        return response;
      });

      await client.get('/users');

      expect(calls).toEqual([
        'first-before',
        'second-before',
        'fetch',
        'second-after',
        'first-after',
      ]);
    });

    it('allows middleware to modify the request', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);

      client.use(async (ctx, next) => {
        ctx.request.headers = { Authorization: 'Bearer token' };
        return next();
      });

      await client.get('/users');

      expect(fetchMock).toHaveBeenCalledWith('/users', {
        method: 'GET',
        headers: { Authorization: 'Bearer token' },
      });
    });

    it('allows middleware to modify the url', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}));

      const client = new HttpClient(fetchMock);

      client.use(async (ctx, next) => {
        ctx.url = '/v2/users';
        return next();
      });

      await client.get('/users');

      expect(fetchMock).toHaveBeenCalledWith('/v2/users', { method: 'GET' });
    });

    it('allows middleware to short-circuit the request', async () => {
      const client = new HttpClient(fetchMock);

      client.use(async () => jsonResponse({ cached: true }));

      const result = await client.get<{ cached: boolean }>('/users');

      expect(result).toEqual({ cached: true });
      expect(fetchMock).not.toHaveBeenCalled();
    });

    it('propagates errors thrown by middleware', async () => {
      const client = new HttpClient(fetchMock);

      client.use(async () => {
        throw new Error('middleware failure');
      });

      await expect(client.get('/users')).rejects.toThrow('middleware failure');
      expect(fetchMock).not.toHaveBeenCalled();
    });

    it('lets an outer middleware inspect the response from an inner middleware', async () => {
      fetchMock.mockResolvedValue(jsonResponse({}, 200));

      const client = new HttpClient(fetchMock);
      const seenStatuses: number[] = [];

      client.use(async (_, next) => {
        const response = await next();
        seenStatuses.push(response.status);
        return response;
      });

      client.use(async () => jsonResponse({ cached: true }, 201));

      await client.get('/users');

      expect(seenStatuses).toEqual([201]);
    });

    it('returns "this" from use() to allow chaining', () => {
      const client = new HttpClient(fetchMock);
      const returned = client.use(async (_, next) => next());

      expect(returned).toBe(client);
    });
  });

  describe('response handling', () => {
    it('throws with status and statusText when the response is unsuccessful', async () => {
      fetchMock.mockResolvedValue(
        new Response('Unauthorized', {
          status: 401,
          statusText: 'Unauthorized',
        }),
      );

      const client = new HttpClient(fetchMock);

      await expect(client.get('/users')).rejects.toThrow('401 Unauthorized');
    });

    it('throws for 5xx server errors', async () => {
      fetchMock.mockResolvedValue(
        new Response('Boom', {
          status: 500,
          statusText: 'Internal Server Error',
        }),
      );

      const client = new HttpClient(fetchMock);

      await expect(client.get('/users')).rejects.toThrow(
        '500 Internal Server Error',
      );
    });

    it('returns undefined for 204 No Content responses', async () => {
      fetchMock.mockResolvedValue(new Response(null, { status: 204 }));

      const client = new HttpClient(fetchMock);
      const result = await client.delete('/users/1');

      expect(result).toBeUndefined();
    });

    it('does not attempt to parse the body on a 204 response', async () => {
      const response = new Response(null, { status: 204 });
      const jsonSpy = vi.spyOn(response, 'json');
      fetchMock.mockResolvedValue(response);

      const client = new HttpClient(fetchMock);
      await client.delete('/users/1');

      expect(jsonSpy).not.toHaveBeenCalled();
    });

    it('parses and returns a JSON body for successful responses', async () => {
      fetchMock.mockResolvedValue(jsonResponse({ id: 1, name: 'John' }));

      const client = new HttpClient(fetchMock);
      const result = await client.get<{ id: number; name: string }>('/users/1');

      expect(result).toEqual({ id: 1, name: 'John' });
    });

    it('propagates rejection when the underlying fetch call fails', async () => {
      fetchMock.mockRejectedValue(new Error('network error'));

      const client = new HttpClient(fetchMock);

      await expect(client.get('/users')).rejects.toThrow('network error');
    });
  });
});
