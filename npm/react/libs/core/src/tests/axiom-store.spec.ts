import { describe, it, expect, vi } from 'vitest';
import { createStore } from '../store/axiom-store';

describe('axiom-store', () => {
  describe('state updates', () => {
    it('starts with a frozen initial state', () => {
      const store = createStore({ count: 0 });

      expect(store.get()).toEqual({ count: 0 });
      expect(Object.isFrozen(store.get())).toBe(true);
    });

    it('merges a changed patch into state and produces a new frozen snapshot', () => {
      const store = createStore({ a: 1, b: 2 });
      const snapshot = store.get();

      store.set(() => ({ b: 20 }));

      expect(store.get()).toEqual({ a: 1, b: 20 });
      expect(store.get()).not.toBe(snapshot);
      expect(Object.isFrozen(store.get())).toBe(true);
    });

    it('passes previous state to the updater and accumulates across sequential calls', () => {
      const store = createStore({ count: 0 });

      store.set((prev) => ({ count: prev.count + 1 }));
      store.set((prev) => ({ count: prev.count + 1 }));
      store.set((prev) => ({ count: prev.count + 1 }));

      expect(store.get().count).toBe(3);
    });

    it('keeps state fully isolated between separate store instances', () => {
      const storeA = createStore({ count: 0 });
      const storeB = createStore({ count: 0 });

      storeA.set(() => ({ count: 5 }));

      expect(storeA.get().count).toBe(5);
      expect(storeB.get().count).toBe(0);
    });

    it('allows setting a property to undefined', () => {
      const store = createStore({ value: 1 });

      store.set(() => ({ value: undefined }));

      expect(store.get()).toEqual({ value: undefined });
    });
  });

  describe('no-op updates', () => {
    it('ignores empty patches', () => {
      const store = createStore({ count: 0 });
      const listener = vi.fn();
      const before = store.get();

      store.subscribe(listener);
      store.set(() => ({}));

      expect(store.get()).toBe(before);
      expect(listener).not.toHaveBeenCalled();
    });

    it('ignores patches whose values are Object.is-equal to current state', () => {
      const store = createStore({ count: 0, label: 'x' });
      const listener = vi.fn();
      const before = store.get();

      store.subscribe(listener);
      store.set(() => ({ count: 0 }));

      expect(store.get()).toBe(before);
      expect(listener).not.toHaveBeenCalled();
    });

    it('treats NaN reset to NaN as unchanged', () => {
      const store = createStore({ value: NaN });
      const listener = vi.fn();

      store.subscribe(listener);
      store.set(() => ({ value: NaN }));

      expect(listener).not.toHaveBeenCalled();
      expect(store.get().value).toBeNaN();
    });

    it('treats +0 and -0 as a real change (Object.is semantics)', () => {
      const store = createStore({ value: 0 });
      const listener = vi.fn();

      store.subscribe(listener);
      store.set(() => ({ value: -0 }));

      expect(listener).toHaveBeenCalledTimes(1);
      expect(Object.is(store.get().value, -0)).toBe(true);
    });

    it('updates when at least one patched value changes', () => {
      const store = createStore({ a: 1, b: 2 });
      const listener = vi.fn();

      store.subscribe(listener);
      store.set(() => ({ a: 1, b: 3 }));

      expect(store.get()).toEqual({ a: 1, b: 3 });
      expect(listener).toHaveBeenCalledTimes(1);
    });
  });

  describe('subscriptions', () => {
    it('notifies every registered listener, but not duplicate registrations', () => {
      const store = createStore({ count: 0 });
      const listenerA = vi.fn();
      const listenerB = vi.fn();

      store.subscribe(listenerA);
      store.subscribe(listenerB);
      store.subscribe(listenerA);

      store.set(() => ({ count: 1 }));

      expect(listenerA).toHaveBeenCalledTimes(1);
      expect(listenerB).toHaveBeenCalledTimes(1);
    });

    it('stops future notifications after unsubscribing, and unsubscribing twice is safe', () => {
      const store = createStore({ count: 0 });
      const listener = vi.fn();
      const unsubscribe = store.subscribe(listener);

      unsubscribe();

      expect(() => unsubscribe()).not.toThrow();

      store.set(() => ({ count: 1 }));

      expect(listener).not.toHaveBeenCalled();
    });

    it('unsubscribes listeners independently', () => {
      const store = createStore({ count: 0 });
      const listenerA = vi.fn();
      const listenerB = vi.fn();

      const unsubscribeA = store.subscribe(listenerA);
      store.subscribe(listenerB);

      unsubscribeA();
      store.set(() => ({ count: 1 }));

      expect(listenerA).not.toHaveBeenCalled();
      expect(listenerB).toHaveBeenCalledTimes(1);
    });

    it('lets a listener unsubscribe itself mid-notification without affecting others', () => {
      const store = createStore({ count: 0 });
      const listenerB = vi.fn();
      const listenerA = vi.fn(() => unsubscribeA());

      const unsubscribeA = store.subscribe(listenerA);
      store.subscribe(listenerB);

      store.set(() => ({ count: 1 }));
      store.set(() => ({ count: 2 }));

      expect(listenerA).toHaveBeenCalledTimes(1);
      expect(listenerB).toHaveBeenCalledTimes(2);
    });

    it('skips listeners subscribed mid-notification for the current update only', () => {
      const store = createStore({ count: 0 });
      const late = vi.fn();

      store.subscribe(() => store.subscribe(late));

      store.set(() => ({ count: 1 }));
      expect(late).not.toHaveBeenCalled();

      store.set(() => ({ count: 2 }));
      expect(late).toHaveBeenCalledTimes(1);
    });

    it('listeners receive the committed state', () => {
      const store = createStore({ count: 0 });

      store.subscribe(() => {
        expect(store.get().count).toBe(1);
      });

      store.set(() => ({ count: 1 }));
    });
  });

  describe('reference equality', () => {
    it('treats a new object/array reference as a state change, even if structurally equal', () => {
      const store = createStore({
        user: { name: 'John' },
        tags: ['a', 'b'],
      });
      const listener = vi.fn();
      const before = store.get();

      store.subscribe(listener);

      store.set(() => ({
        user: { name: 'John' },
        tags: ['a', 'b'],
      }));

      expect(listener).toHaveBeenCalledTimes(1);
      expect(store.get()).not.toBe(before);
      expect(store.get().user).not.toBe(before.user);
      expect(store.get().tags).not.toBe(before.tags);
    });

    it('ignores unchanged object/array references', () => {
      const user = { name: 'John' };
      const tags = ['a', 'b'];
      const store = createStore({ user, tags });
      const listener = vi.fn();
      const before = store.get();

      store.subscribe(listener);

      store.set(() => ({ user, tags }));

      expect(store.get()).toBe(before);
      expect(listener).not.toHaveBeenCalled();
    });
  });

  describe('error handling', () => {
    it('propagates when updater throws, leaving state unchanged', () => {
      const store = createStore({ count: 0 });
      const before = store.get();

      expect(() =>
        store.set(() => {
          throw new Error('boom');
        }),
      ).toThrow('boom');

      expect(store.get()).toBe(before);
    });

    it('propagates a throwing listener and does not call listeners registered after it', () => {
      const store = createStore({ count: 0 });
      const bad = vi.fn(() => {
        throw new Error('listener boom');
      });
      const after = vi.fn();

      store.subscribe(bad);
      store.subscribe(after);

      expect(() => store.set(() => ({ count: 1 }))).toThrow('listener boom');
      expect(after).not.toHaveBeenCalled();
      expect(store.get().count).toBe(1);
    });
  });
});
