import { Moon, Sun } from 'lucide-react';
import { useTheme } from '@axiomframework/react-core';

import { Button } from './ui/button';

export const THEME_OPTIONS = [
  { value: 'light', icon: Sun, label: 'AxiomTheme:Light' },
  { value: 'dark', icon: Moon, label: 'AxiomTheme:Dark' },
] as const;

export function ThemeToggle() {
  const { activeTheme, setTheme } = useTheme();

  const isDark = activeTheme === 'dark';
  const ThemeIcon = isDark ? Moon : Sun;

  const toggleTheme = () => {
    setTheme(isDark ? 'light' : 'dark');
  };

  return (
    <Button
      variant="outline"
      size="icon"
      className="cursor-pointer"
      onClick={toggleTheme}
    >
      <ThemeIcon className="size-4" />
    </Button>
  );
}
