import { useSyncExternalStore } from 'react';
import type { AxiomStore } from '../models/common';

export interface AxiomStoreHook<T> {
  <TSelected>(selector: (state: Readonly<T>) => TSelected): TSelected;
  get(): Readonly<T>;
}

export function createStore<T extends object>(initialState: T): AxiomStore<T> {
  let state = Object.freeze(initialState);
  const listeners = new Set<VoidFunction>();

  return {
    get() {
      return state;
    },

    set(updater) {
      const patch = updater(state);
      const keys = Object.keys(patch) as (keyof T)[];

      if (keys.length === 0) {
        return;
      }

      let changed = false;
      for (const key of keys) {
        if (!Object.is(state[key], patch[key])) {
          changed = true;
          break;
        }
      }

      if (!changed) {
        return;
      }

      state = Object.freeze({ ...state, ...patch });

      for (const listener of [...listeners]) {
        listener();
      }
    },

    subscribe(listener) {
      listeners.add(listener);
      return () => listeners.delete(listener);
    },
  };
}

export function createStoreHook<T extends object>(
  store: AxiomStore<T>,
): AxiomStoreHook<T> {
  function useStore<TSelected>(
    selector: (state: Readonly<T>) => TSelected,
  ): TSelected {
    return useSyncExternalStore(
      store.subscribe,
      () => selector(store.get()),
      () => selector(store.get()),
    );
  }

  useStore.get = store.get;

  return useStore;
}
