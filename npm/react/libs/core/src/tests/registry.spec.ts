import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApplicationInitializer } from '../models/application';
import {
  clearInitializers,
  getInitializers,
  provideInitializers,
} from '../initializer/registry';

const makeInitializer = (
  overrides: Partial<ApplicationInitializer> = {},
): ApplicationInitializer => ({
  configure: vi.fn(),
  ...overrides,
});

beforeEach(() => {
  clearInitializers();
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('initializer registry', () => {
  it('starts with no initializers', () => {
    expect(getInitializers()).toEqual([]);
  });

  describe('provideInitializers', () => {
    it('registers an initializer that has a configure fn', () => {
      const initializer = makeInitializer();

      provideInitializers(initializer);

      expect(getInitializers()).toEqual([initializer]);
    });

    it('registers an initializer that has only postConfigure', () => {
      const initializer = makeInitializer({
        configure: undefined,
        postConfigure: vi.fn(),
      });

      provideInitializers(initializer);

      expect(getInitializers()).toEqual([initializer]);
    });

    it('preserves insertion order across separate calls', () => {
      const a = makeInitializer();
      const b = makeInitializer();
      const c = makeInitializer();

      provideInitializers(a, b);
      provideInitializers(c);

      expect(getInitializers()).toEqual([a, b, c]);
    });

    it('rejects an initializer with neither configure nor postConfigure', () => {
      const initializer = makeInitializer({
        configure: undefined,
        postConfigure: undefined,
        platform: 'server',
      });

      provideInitializers(initializer);

      expect(getInitializers()).toEqual([]);
    });

    it('keeps valid initializers from a call that also contains an invalid one', () => {
      const valid = makeInitializer();
      const invalid = makeInitializer({
        configure: undefined,
        postConfigure: undefined,
      });

      provideInitializers(valid, invalid);

      expect(getInitializers()).toEqual([valid]);
    });

    it('is idempotent for the same initializer reference', () => {
      const initializer = makeInitializer();

      provideInitializers(initializer);
      provideInitializers(initializer);

      expect(getInitializers()).toHaveLength(1);
    });

    it('does not dedupe two distinct objects with identical shape', () => {
      const a = makeInitializer();
      const b = makeInitializer();

      provideInitializers(a, b);

      expect(getInitializers()).toHaveLength(2);
    });

    it('accepts a call with zero arguments without throwing', () => {
      expect(() => provideInitializers()).not.toThrow();
      expect(getInitializers()).toEqual([]);
    });
  });

  describe('getInitializers', () => {
    it('returns a snapshot array, not a live view of the registry', () => {
      const snapshot = getInitializers();

      provideInitializers(makeInitializer());

      expect(snapshot).toEqual([]);
      expect(getInitializers()).toHaveLength(1);
    });
  });

  describe('clearInitializers', () => {
    it('empties a populated registry', () => {
      provideInitializers(makeInitializer(), makeInitializer());

      expect(getInitializers()).toHaveLength(2);

      clearInitializers();

      expect(getInitializers()).toEqual([]);
    });

    it('is safe to call when the registry is already empty', () => {
      expect(() => clearInitializers()).not.toThrow();
      expect(getInitializers()).toEqual([]);
    });

    it('allows new initializers to be registered again after clearing', () => {
      provideInitializers(makeInitializer());

      clearInitializers();

      const initializer = makeInitializer();

      provideInitializers(initializer);

      expect(getInitializers()).toEqual([initializer]);
    });
  });
});
