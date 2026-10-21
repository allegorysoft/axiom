import { useEffect, useLayoutEffect } from 'react';
import type { Preferences } from './preferences';
import { COLOR_THEME_OPTIONS, FONT_OPTIONS } from './options';
import { createStoreHook } from '@axiomframework/react-core';
import { preferencesStore } from './preferences-store';

export const usePreferencesStore = createStoreHook(preferencesStore);

export function usePreferences(): Preferences {
  return usePreferencesStore((s) => s.preferences);
}

export function useColorTheme() {
  const preferences = usePreferences();

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

export function useFont() {
  const preferences = usePreferences();

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

export function useScale() {
  const preferences = usePreferences();

  useLayoutEffect(() => {
    document.documentElement.dataset.scale = preferences.scale;
  }, [preferences.scale]);
}

export function useRadius() {
  const preferences = usePreferences();

  useEffect(() => {
    document.documentElement.dataset.radius = preferences.radius;
  }, [preferences.radius]);
}
