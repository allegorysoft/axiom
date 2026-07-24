import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { Environment } from '../models/environment';
import { useEnvironmentStore } from '../stores/environment.store';

const makeEnvironment = (
  overrides: Partial<Environment> = {},
): Environment => ({
  production: false,
  endpoints: {
    users: { url: 'https://api.test/users' },
    orders: { url: 'https://api.test/orders' },
  },
  oauth: {
    authority: 'https://auth.test',
    clientId: 'axiom-web',
    scope: 'openid profile',
    responseType: 'code',
    redirectUri: 'https://app.test/callback',
  },
  ...overrides,
});

const { setEnvironment, patchEndpoints, patchOAuth, reset } =
  useEnvironmentStore.getState();

const getEnvironment = () => useEnvironmentStore.getState().environment;

const spyOnWarn = () =>
  vi.spyOn(console, 'warn').mockImplementation(() => undefined);

beforeEach(() => {
  useEnvironmentStore.setState({ environment: undefined });
});

afterEach(() => {
  vi.restoreAllMocks();
  vi.unstubAllEnvs();
});

describe('useEnvironmentStore', () => {
  it('starts with no environment', () => {
    expect(getEnvironment()).toBeUndefined();
  });

  describe('setEnvironment', () => {
    it('stores the environment', () => {
      const environment = makeEnvironment();
      setEnvironment(environment);
      expect(getEnvironment()).toBe(environment);
    });

    it('replaces a previously set environment instead of merging it', () => {
      const init = makeEnvironment();
      setEnvironment(init);

      const next = makeEnvironment({
        production: true,
        endpoints: { health: { url: 'https://api.test/health' } },
      });
      setEnvironment(next);

      expect(getEnvironment()).toBe(next);
      expect(getEnvironment()?.endpoints).toEqual({
        health: { url: 'https://api.test/health' },
      });
    });
  });

  describe('patchEndpoints', () => {
    it('adds new endpoints while keeping the existing ones', () => {
      setEnvironment(makeEnvironment());
      patchEndpoints({ health: { url: 'https://api.test/health' } });
      expect(getEnvironment()?.endpoints).toEqual({
        users: { url: 'https://api.test/users' },
        orders: { url: 'https://api.test/orders' },
        health: { url: 'https://api.test/health' },
      });
    });

    it('replaces an endpoint that already exists', () => {
      setEnvironment(makeEnvironment());
      patchEndpoints({ users: { url: 'https://api.test/v2/users' } });
      expect(getEnvironment()?.endpoints.users).toEqual({
        url: 'https://api.test/v2/users',
      });
    });

    it('leaves the rest of the environment untouched', () => {
      const environment = makeEnvironment();

      setEnvironment(environment);
      patchEndpoints({ health: { url: 'https://api.test/health' } });

      expect(getEnvironment()?.production).toBe(false);
      expect(getEnvironment()?.oauth).toBe(environment.oauth);
    });

    it('does not mutate the previous environment', () => {
      const environment = makeEnvironment();

      setEnvironment(environment);
      patchEndpoints({ users: { url: 'https://api.test/v2/users' } });

      expect(environment.endpoints.users.url).toBe('https://api.test/users');
      expect(getEnvironment()).not.toBe(environment);
      expect(getEnvironment()?.oauth).toBe(environment.oauth);
      expect(getEnvironment()?.endpoints).not.toBe(environment.endpoints);
    });

    it('is a no-op and warns when the environment is not initialised', () => {
      const warn = spyOnWarn();

      patchEndpoints({ health: { url: 'https://api.test/health' } });

      expect(getEnvironment()).toBeUndefined();
      expect(warn).toHaveBeenCalledOnce();
      expect(warn.mock.calls[0][0]).toContain('patchEndpoints');
    });
  });

  describe('patchOAuth', () => {
    it('merges the given fields into the existing oauth config', () => {
      setEnvironment(makeEnvironment());
      patchOAuth({ clientId: 'axiom-mobile', responseType: 'password' });
      expect(getEnvironment()?.oauth).toStrictEqual({
        authority: 'https://auth.test',
        clientId: 'axiom-mobile',
        scope: 'openid profile',
        responseType: 'password',
        redirectUri: 'https://app.test/callback',
      });
    });

    it('accepts an empty patch without changing anything', () => {
      const environment = makeEnvironment();

      setEnvironment(environment);
      patchOAuth({});

      expect(getEnvironment()?.oauth).toStrictEqual(environment.oauth);
    });

    it('leaves the endpoints untouched', () => {
      const environment = makeEnvironment();

      setEnvironment(environment);
      patchOAuth({ scope: 'email' });

      expect(getEnvironment()?.endpoints).toBe(environment.endpoints);
      expect(getEnvironment()?.oauth.scope).toBe('email');
    });

    it('does not mutate the previous oauth config', () => {
      const environment = makeEnvironment();

      setEnvironment(environment);
      patchOAuth({ clientId: 'axiom-mobile' });

      expect(environment.oauth.clientId).toBe('axiom-web');
      expect(getEnvironment()?.oauth).not.toBe(environment.oauth);
    });

    it('is a no-op and warns when the environment is not initialised', () => {
      const warn = spyOnWarn();

      patchOAuth({ clientId: 'axiom-mobile' });

      expect(getEnvironment()).toBeUndefined();
      expect(warn).toHaveBeenCalledOnce();
      expect(warn.mock.calls[0][0]).toContain('patchOAuth');
    });
  });

  describe('reset', () => {
    it('clears the environment', () => {
      setEnvironment(makeEnvironment());
      reset();
      expect(getEnvironment()).toBeUndefined();
    });

    it('is safe to call when nothing was set', () => {
      expect(() => reset()).not.toThrow();
      expect(getEnvironment()).toBeUndefined();
    });
  });

  describe('subscribers', () => {
    it('notifies listeners when the environment changes', () => {
      const listener = vi.fn();
      const unsubscribe = useEnvironmentStore.subscribe(listener);

      setEnvironment(makeEnvironment());
      patchOAuth({ clientId: 'axiom-mobile' });

      expect(listener).toHaveBeenCalledTimes(2);
      unsubscribe();
    });

    it('does not notify listeners when a patch is rejected', () => {
      spyOnWarn(); //Avoid console warn for environment initialize
      const listener = vi.fn();
      const unsubscribe = useEnvironmentStore.subscribe(listener);

      patchEndpoints({ health: { url: 'https://api.test/health' } });
      patchOAuth({ clientId: 'axiom-mobile' });

      expect(listener).not.toHaveBeenCalled();
      unsubscribe();
    });
  });

  describe('in production builds', () => {
    beforeEach(() => {
      vi.stubEnv('PROD', true);
    });

    it('stays silent when the environment is not initialised', () => {
      const warn = spyOnWarn();

      patchEndpoints({ health: { url: 'https://api.test/health' } });
      patchOAuth({ clientId: 'axiom-mobile' });

      expect(warn).not.toHaveBeenCalled();
      expect(getEnvironment()).toBeUndefined();
    });
  });
});
