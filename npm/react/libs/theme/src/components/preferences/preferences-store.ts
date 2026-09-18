import { createStore } from '@axiomframework/react-core';

import type {
  Preferences,
  PreferencesState,
  PreferencesStore,
} from './preferences';

export const defaultPreferences: Preferences = {
  themePreset: 'default',
  font: 'geist',
  navbarBehavior: 'sticky',
  sidebarStyle: 'floating',
  radius: '0.625rem',
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
