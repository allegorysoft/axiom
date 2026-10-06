import { createStore } from '@axiomframework/react-core';
import type {
  Preferences,
  PreferencesState,
  PreferencesStore,
} from './preferences';
import {
  loadPreferences,
  savePreferences,
  clearPreferences,
  onPreferencesChange,
} from './preferences-storage';

export const defaultPreferences: Preferences = {
  colorTheme: 'axiom',
  font: 'inter',
  navbarBehavior: 'sticky',
  sidebarStyle: 'floating',
  userMenuPosition: 'navbar',
  radius: 'md',
  scale: 'md',
};

const baseStore = createStore<PreferencesState>({
  preferences: loadPreferences(defaultPreferences),
});

function commit(next: Preferences): void {
  baseStore.set(() => ({ preferences: next }));
  savePreferences(next);
}

export const preferencesStore: PreferencesStore = Object.assign(baseStore, {
  setPreferences(preferences: Preferences): void {
    commit(preferences);
  },

  patchPreferences(patch: Partial<Preferences>): void {
    commit({ ...baseStore.get().preferences, ...patch });
  },

  resetPreferences(): void {
    clearPreferences();
    baseStore.set(() => ({ preferences: defaultPreferences }));
  },
});

// Optional: cross-tab sync (call once from your app entrypoint)
export function initPreferencesSync(): () => void {
  return onPreferencesChange((next) => {
    if (next) {
      baseStore.set(() => ({ preferences: next }));
    } else {
      baseStore.set(() => ({ preferences: defaultPreferences }));
    }
  });
}
