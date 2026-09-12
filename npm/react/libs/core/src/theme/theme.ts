import { CookieOptions, getCookie, setCookie } from '../storage/cookie-storage';

export type Theme = 'light' | 'dark' | 'system';
/** A theme that has been resolved — never 'system'. */
export type ActiveTheme = Exclude<Theme, 'system'>;

const COOKIE_NAME = 'axiom_theme';

export function getTheme(): Theme {
  return getCookie<Theme>(COOKIE_NAME) ?? 'system';
}

export function setTheme(theme: Theme, options?: CookieOptions) {
  setCookie<Theme>(COOKIE_NAME, theme, options);
}

export function getSystemTheme(): ActiveTheme {
  if (typeof window === 'undefined' || !window.matchMedia) {
    return 'light';
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light';
}
