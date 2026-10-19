import { useEffect } from 'react';
import type { Preferences } from './preferences';
import { FONT_OPTIONS } from './options';

export function useFont(preferences: Preferences) {
  useEffect(() => {
    document.documentElement.dataset.font = preferences.font;

    document.head
      .querySelectorAll('link[data-font]')
      .forEach((link) => link.remove());

    const href = FONT_OPTIONS.find((f) => f.value === preferences.font)?.url;
    if (!href) {
      return;
    }

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    link.dataset.font = preferences.font;
    document.head.appendChild(link);
  }, [preferences.font]);
}
