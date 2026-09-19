import { HugeiconsIcon } from '@hugeicons/react';
import { MoonIcon as Moon, SunIcon as Sun } from '@hugeicons/core-free-icons';
import { useTheme } from '@axiomframework/react-core';

import { Button } from '../ui/button';

export function ThemeToggle() {
  const { activeTheme, setTheme } = useTheme();

  const isDark = activeTheme === 'dark';
  const ThemeIcon = isDark ? Sun : Moon;

  const toggleTheme = () => setTheme(isDark ? 'light' : 'dark');

  return (
    <Button
      variant="outline"
      size="icon"
      className="cursor-pointer"
      onClick={toggleTheme}
    >
      <HugeiconsIcon icon={ThemeIcon} strokeWidth={2} className="size-4" />
    </Button>
  );
}
