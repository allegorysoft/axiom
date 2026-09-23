import { createStore } from '@axiomframework/react-core';

import type {
  Preferences,
  PreferencesState,
  PreferencesStore,
} from './preferences';

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
  preferences: defaultPreferences,
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
