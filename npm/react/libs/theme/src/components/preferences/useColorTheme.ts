import { useLayoutEffect } from 'react';
import type { Preferences } from './preferences';
import { COLOR_THEME_OPTIONS } from './options';

export function useColorTheme(preferences: Preferences) {
  useLayoutEffect(() => {
    const colorTheme = preferences.colorTheme;

    document.documentElement.dataset.colorTheme = colorTheme;

    const href = COLOR_THEME_OPTIONS.find((f) => f.value === colorTheme)?.url;
    let link = document.head.querySelector<HTMLLinkElement>(
      'link[data-color-theme]',
    );

    if (colorTheme === 'default' || !href) {
      link?.remove();
      return;
    }

    if (!link) {
      link = document.createElement('link');
      link.rel = 'stylesheet';
      document.head.appendChild(link);
    }

    link.dataset.colorTheme = colorTheme;
    link.href = href;
  }, [preferences.colorTheme]);
}
