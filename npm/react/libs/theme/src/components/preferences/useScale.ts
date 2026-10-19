import { useLayoutEffect } from 'react';
import type { Preferences } from './preferences';

export function useScale(preferences: Preferences) {
  useLayoutEffect(() => {
    document.documentElement.dataset.scale = preferences.scale;
  }, [preferences.scale]);
}
