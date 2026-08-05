import { useCallback, useSyncExternalStore } from 'react';
import type { AxiomStore } from '../models/common';

export interface AxiomStoreHook<T> {
  <TSelected>(selector: (state: Readonly<T>) => TSelected): TSelected;
  getState(): Readonly<T>;
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
      if (Object.keys(patch).length === 0) {
        return;
      }

      let changed = false;
      for (const key of Object.keys(patch) as (keyof T)[]) {
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
    const getSnapshot = useCallback(() => selector(store.get()), [selector]);

    return useSyncExternalStore(store.subscribe, getSnapshot, getSnapshot);
  }

  useStore.getState = store.get;

  return useStore;
}
