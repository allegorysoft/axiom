import { createStoreHook } from '@axiomframework/react-core';
import { preferencesStore } from './preferences-store';

export const usePreferences = createStoreHook(preferencesStore);
