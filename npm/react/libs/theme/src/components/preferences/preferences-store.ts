import { createStore } from '@axiomframework/react-core';

import type {
  Preferences,
  PreferencesState,
  PreferencesStore,
} from './preferences';

const STORAGE_KEY = 'axiom.preferences';

export const defaultPreferences: Preferences = {
  colorTheme: 'default',
  font: 'manrope',
  navbarBehavior: 'sticky',
  sidebarStyle: 'floating',
  userMenuPosition: 'navbar',
  radius: 'md',
  scale: 'md',
};

const baseStore = createStore<PreferencesState>({
  preferences: readFromStorage(),
});

export const preferencesStore: PreferencesStore = Object.assign(baseStore, {
  setPreferences(preferences: Preferences): void {
    baseStore.set(() => ({ preferences }));
  },

  patchPreferences(preferences: Partial<Preferences>): void {
    baseStore.set((state) => ({
      preferences: { ...state.preferences, ...preferences },
    }));
  },

  resetPreferences(): void {
    baseStore.set(() => ({ preferences: defaultPreferences }));
  },
});

baseStore.subscribe(() => {
  try {
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify(baseStore.get().preferences),
    );
  } catch {}
});

function readFromStorage(): Preferences {
  try {
    const saved = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '{}');
    return { ...defaultPreferences, ...saved };
  } catch {
    return defaultPreferences;
  }
}
