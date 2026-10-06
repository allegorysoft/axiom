import {
  readLocalStorage,
  writeLocalStorage,
  removeLocalStorage,
  onLocalStorageChange,
} from '@axiomframework/react-core';
import type { Preferences } from './preferences';

const STORAGE_KEY = 'axiom_preferences';

export function loadPreferences(fallback: Preferences): Preferences {
  return readLocalStorage<Preferences>(STORAGE_KEY, fallback, (stored, fb) => ({
    ...fb,
    ...stored,
  }));
}

export function savePreferences(preferences: Preferences): void {
  writeLocalStorage<Preferences>(STORAGE_KEY, preferences);
}

export function clearPreferences(): void {
  removeLocalStorage(STORAGE_KEY);
}

export function onPreferencesChange(
  handler: (preferences: Preferences | null) => void,
): () => void {
  return onLocalStorageChange<Preferences>(STORAGE_KEY, handler);
}
