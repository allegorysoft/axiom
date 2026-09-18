import { createStore } from '@axiomframework/react-core';
import type {
  Preferences,
  PreferencesState,
  PreferencesStore,
} from './preferences';

export const defaultPreferences: Preferences = {
  themePreset: {
    name: 'Default',
    color: '#000000',
  },
  font: {
    value: 'geist',
    title: 'Geist',
  },
  navbarBehavior: 'sticky',
  sidebarStyle: 'floating',
  radius: {
    name: 'md',
    value: '0.5rem',
  },
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
