import React, {
  createContext,
  useContext,
  useEffect,
  useState,
  useCallback,
  useMemo,
} from 'react';

import type { CookieOptions } from '../storage/cookie-storage';
import {
  type Theme,
  type ActiveTheme,
  getTheme,
  getSystemTheme,
  setTheme as setCookieTheme,
} from './theme';

interface ThemeContextValue {
  theme: Theme;
  activeTheme: ActiveTheme;
  setTheme: (theme: Theme, options?: CookieOptions) => void;
  toggleTheme: () => void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

export interface ThemeProviderProps {
  children: React.ReactNode;
  cookieOptions?: CookieOptions;
}

const CYCLE: Theme[] = ['light', 'dark', 'system'];

export function ThemeProvider({ children, cookieOptions }: ThemeProviderProps) {
  const [theme, setTheme] = useState<Theme>(getTheme);
  const [systemTheme, setSystemTheme] = useState<ActiveTheme>(getSystemTheme);

  const activeTheme = theme === 'system' ? systemTheme : theme;

  useEffect(() => {
    if (!window.matchMedia) {
      return;
    }

    const media = window.matchMedia('(prefers-color-scheme: dark)');
    const onChange = () => setSystemTheme(media.matches ? 'dark' : 'light');
    media.addEventListener('change', onChange);

    return () => media.removeEventListener('change', onChange);
  }, []);

  useEffect(() => {
    document.documentElement.classList.toggle('dark', activeTheme === 'dark');
  }, [activeTheme]);

  const updateTheme = useCallback(
    (next: Theme, options?: CookieOptions) => {
      setTheme(next);
      setCookieTheme(next, options ?? cookieOptions);
    },
    [cookieOptions],
  );

  const toggleTheme = useCallback(() => {
    const next = CYCLE[(CYCLE.indexOf(theme) + 1) % CYCLE.length];
    updateTheme(next);
  }, [theme, updateTheme]);

  const value = useMemo<ThemeContextValue>(
    () => ({ theme, activeTheme, setTheme: updateTheme, toggleTheme }),
    [theme, activeTheme, updateTheme, toggleTheme],
  );

  return (
    <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
  );
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error('useTheme must be used within a ThemeProvider');
  return ctx;
}
