/**
 * @vitest-environment jsdom
 */
/// <reference lib="dom" />

import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  localizerStore,
  setCultureReloadHandler,
} from '../localization/localizer-store';

describe('localizer-store', () => {
  beforeEach(() => {
    localizerStore.reset();
    setCultureReloadHandler(() => {});
  });

  describe('state', () => {
    it('exposes the current localization state', () => {
      expect(localizerStore.get()).toEqual({
        translations: {},
        culture: {
          name: 'en',
          displayName: 'English',
        },
        status: 'idle',
        error: null,
      });
    });

    it('allows direct state patching', () => {
      localizerStore.set((state) => ({
        ...state,
        status: 'ready',
      }));

      expect(localizerStore.get().status).toBe('ready');
    });
  });

  describe('culture', () => {
    it('updates the current culture', () => {
      localizerStore.setCulture({
        name: 'tr',
        displayName: 'Türkçe',
      });

      expect(localizerStore.get().culture).toEqual({
        name: 'tr',
        displayName: 'Türkçe',
      });
    });

    it('notifies subscribers when the culture changes', () => {
      const listener = vi.fn();
      localizerStore.subscribe(listener);

      localizerStore.setCulture({
        name: 'tr',
        displayName: 'Türkçe',
      });

      expect(listener).toHaveBeenCalledTimes(1);
    });
  });

  describe('status', () => {
    it('updates the localization status', () => {
      localizerStore.setStatus('loading');

      expect(localizerStore.get().status).toBe('loading');
      expect(localizerStore.get().error).toBeNull();
    });

    it('stores the associated error', () => {
      const error = new Error('Failed');

      localizerStore.setStatus('error', error);

      expect(localizerStore.get().status).toBe('error');
      expect(localizerStore.get().error).toBe(error);
    });
  });

  describe('translations', () => {
    it('sets translations', () => {
      localizerStore.setTranslations({
        Default: {
          Greeting: 'Hello',
        },
      });

      expect(localizerStore.get().translations.Default?.Greeting).toBe('Hello');
    });

    it('merges translations', () => {
      localizerStore.setTranslations({
        Default: {
          Greeting: 'Hello',
          GreetingWithParam: 'Hello, {name}',
        },
      });

      localizerStore.setTranslations({
        Checkout: {
          Total: 'Total',
        },
      });

      expect(localizerStore.get().translations).toEqual({
        Default: {
          Greeting: 'Hello',
          GreetingWithParam: 'Hello, {name}',
        },
        Checkout: {
          Total: 'Total',
        },
      });
    });

    it('overwrites existing translations by default', () => {
      localizerStore.setTranslations({
        Default: {
          Greeting: 'Hello',
        },
      });

      localizerStore.setTranslations({
        Default: {
          Greeting: 'Hi',
        },
      });

      expect(localizerStore.get().translations.Default?.Greeting).toBe('Hi');
    });

    it('notifies subscribers when translations change', () => {
      const listener = vi.fn();
      localizerStore.subscribe(listener);

      localizerStore.setTranslations({
        Default: {
          Greeting: 'Hello',
        },
      });

      expect(listener).toHaveBeenCalledTimes(1);
    });
  });

  describe('localize', () => {
    beforeEach(() => {
      localizerStore.setTranslations({
        Default: {
          Welcome: 'Welcome',
          Hello: 'Hello, {name}!',
        },
        Checkout: {
          Total: 'Total: {0}',
        },
      });
    });

    it('returns translated text', () => {
      expect(localizerStore.localize('Welcome')).toBe('Welcome');
    });

    it('falls back to the key when translation is missing', () => {
      expect(localizerStore.localize('Missing')).toBe('Missing');
    });

    it('formats translated text with arguments', () => {
      expect(localizerStore.localize('Default:Hello', { name: 'Ada' })).toBe(
        'Hello, Ada!',
      );

      expect(localizerStore.localize('Checkout:Total', ['$42'])).toBe(
        'Total: $42',
      );
    });
  });

  describe('reload', () => {
    it('invokes the reload handler when culture changes', () => {
      const handler = vi.fn();

      setCultureReloadHandler(handler);

      const culture = {
        name: 'tr',
        displayName: 'Türkçe',
      };

      localizerStore.setCulture(culture);

      expect(document.documentElement.lang).toBe('tr');
      expect(handler).toHaveBeenCalledTimes(1);
      expect(handler).toHaveBeenCalledWith(culture);
    });

    it('does not invoke the reload handler when culture is unchanged', () => {
      const handler = vi.fn();

      setCultureReloadHandler(handler);

      localizerStore.setCulture({
        name: 'en',
        displayName: 'English',
      });

      expect(handler).not.toHaveBeenCalled();
    });
  });
});
