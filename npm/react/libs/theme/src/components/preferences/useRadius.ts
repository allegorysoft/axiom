import { useEffect } from 'react';
import { Preferences } from './preferences';

export function useRadius(preferences: Preferences) {
  useEffect(() => {
    document.documentElement.dataset.radius = preferences.radius;
  }, [preferences.radius]);
}
