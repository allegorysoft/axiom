import { HugeiconsIcon, type IconSvgElement } from '@hugeicons/react';
import {
  ChevronLeftIcon as ChevronLeft,
  MonitorIcon as Monitor,
  Moon02Icon as Moon,
  SunIcon as Sun,
} from '@hugeicons/core-free-icons';

import {
  type Theme,
  THEME_OPTIONS,
  useTheme,
  useTranslation,
} from '@axiomframework/react-core';

import {
  DropdownMenuItem,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
} from '../ui/dropdown-menu';

const THEME_ICONS: Record<Theme, IconSvgElement> = {
  light: Sun,
  dark: Moon,
  system: Monitor,
};
export function ThemeSelect({ onBack }: { onBack: () => void }) {
  const t = useTranslation();
  const { theme, setTheme } = useTheme();

  return (
    <>
      <DropdownMenuItem
        className="gap-2 px-2 py-2 font-semibold"
        closeOnClick={false}
        onClick={onBack}
        aria-label="Back to user menu"
      >
        <HugeiconsIcon icon={ChevronLeft} strokeWidth={2} />
        <span>{t('AxiomTheme:Theme')}</span>
      </DropdownMenuItem>

      <DropdownMenuSeparator className="mx-1 my-1" />

      <DropdownMenuRadioGroup
        value={theme}
        onValueChange={(value) => setTheme(value as Theme)}
        aria-label="Theme"
      >
        {THEME_OPTIONS.map(({ value, label }) => {
          const Icon = THEME_ICONS[value];
          return (
            <DropdownMenuRadioItem
              key={value}
              value={value}
              className="gap-2 py-2"
              closeOnClick={false}
            >
              <HugeiconsIcon icon={Icon} strokeWidth={2} />
              <span>{t(label)}</span>
            </DropdownMenuRadioItem>
          );
        })}
      </DropdownMenuRadioGroup>
    </>
  );
}
