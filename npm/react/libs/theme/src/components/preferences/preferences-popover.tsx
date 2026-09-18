import { useEffect, useLayoutEffect, useRef } from 'react';
import { Palette, CircleOff, LucideIcon } from 'lucide-react';

import { useTranslation } from '@axiomframework/react-core';

import { cn } from '@axiomframework/react-theme/lib/utils';
import { Button } from '../ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';
import { Label } from '../ui/label';
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from '../ui/popover';

import type {
  BaseSize,
  NavbarBehavior,
  RadiusSize,
  SidebarVariants,
} from './preferences';
import { preferencesStore } from './preferences-store';
import { usePreferences } from './use-preferences';

type SegmentedOption = {
  label: string;
  value: string;
  icon?: LucideIcon;
};

const PRESET_OPTIONS = [
  { name: 'Default', color: '#000000' },
  { name: 'Neutral', color: '#808080' },
  { name: 'Vibrant', color: '#FF0000' },
] as const;

const FONT_OPTIONS = [
  { value: 'geist', label: 'Geist' },
  { value: 'system', label: 'System' },
  { value: 'inter', label: 'Inter' },
] as const;

const SEGMENTED_OPTIONS: Record<string, SegmentedOption[]> = {
  navbar: [
    { value: 'sticky', label: 'Sticky' },
    { value: 'scroll', label: 'Scroll' },
  ],
  sidebar: [
    { value: 'inset', label: 'Inset' },
    { value: 'sidebar', label: 'Sidebar' },
    { value: 'floating', label: 'Floating' },
  ],
  radius: [
    { value: '0', label: 'None', icon: CircleOff },
    { value: '0.375rem', label: 'SM' },
    { value: '0.5rem', label: 'MD' },
    { value: '1rem', label: 'LG' },
  ],
  scale: [
    { value: 'sm', label: 'SM' },
    { value: 'md', label: 'MD' },
    { value: 'lg', label: 'LG' },
  ],
};

export function PreferencesPopover() {
  const t = useTranslation();
  const preferences = usePreferences((state) => state.preferences);

  const themePresetClass = getThemePresetClass(preferences.themePreset.name);
  const previousThemePresetClassRef = useRef<string | null>(null);

  useLayoutEffect(() => {
    const root = document.documentElement;
    const previous = previousThemePresetClassRef.current;

    if (previous !== themePresetClass) {
      const others = [...root.classList].filter(
        (c) => c !== previous && c !== themePresetClass,
      );

      root.className = [themePresetClass, ...others].join(' ');
      previousThemePresetClassRef.current = themePresetClass;
    }
  }, [themePresetClass]);

  useEffect(() => {
    document.documentElement.style.setProperty(
      '--radius',
      preferences.radius.value,
    );
  }, [preferences.radius.name]);

  return (
    <Popover>
      <PopoverTrigger
        render={
          <Button
            variant="outline"
            size="icon"
            className="cursor-pointer"
            aria-label="Preferences"
          >
            <Palette />
          </Button>
        }
      />

      <PopoverContent align="start">
        <PopoverHeader>
          <PopoverTitle>{t('AxiomTheme:Preferences')}</PopoverTitle>
          <PopoverDescription>
            {t('AxiomTheme:PreferencesDescription')}
          </PopoverDescription>
        </PopoverHeader>

        <div className="flex flex-col gap-3">
          <Label className="flex-col items-stretch gap-1.5 text-xs font-semibold">
            {t('AxiomTheme:ThemePreset')}
            <Select
              value={preferences.themePreset.name}
              onValueChange={(value) => {
                if (value) {
                  preferencesStore.patchPreferences({
                    themePreset: {
                      ...preferences.themePreset,
                      name: value,
                      color:
                        PRESET_OPTIONS.find((option) => option.name === value)
                          ?.color || '',
                    },
                  });
                }
              }}
            >
              <SelectTrigger className="w-full" size="sm">
                <SelectValue
                  render={() => (
                    <div className="flex items-center gap-2">
                      <span
                        className="w-2 h-2 rounded-full"
                        style={{
                          backgroundColor: preferences.themePreset.color,
                        }}
                      />
                      {preferences.themePreset.name}
                    </div>
                  )}
                />
              </SelectTrigger>

              <SelectContent>
                {PRESET_OPTIONS.map((option) => (
                  <SelectItem key={option.name} value={option.name}>
                    <div className="flex items-center gap-2">
                      <span
                        className="w-2 h-2 rounded-full"
                        style={{ backgroundColor: option.color }}
                      />
                      {option.name}
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Label>

          <Label className="flex-col items-stretch gap-1.5 text-xs font-semibold">
            {t('AxiomTheme:Font')}
            <Select
              value={preferences.font.value}
              onValueChange={(value) => {
                if (value) {
                  preferencesStore.patchPreferences({
                    font: {
                      value,
                      title:
                        FONT_OPTIONS.find((option) => option.value === value)
                          ?.label || '',
                    },
                  });
                }
              }}
            >
              <SelectTrigger className="w-full" size="sm">
                <SelectValue
                  render={() => (
                    <span className="flex items-center gap-2">
                      {preferences.font.title}
                    </span>
                  )}
                />
              </SelectTrigger>
              <SelectContent>
                {FONT_OPTIONS.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Label>

          <SegmentedControl
            label="Navbar Behavior"
            options={SEGMENTED_OPTIONS.navbar}
            value={preferences.navbarBehavior}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                navbarBehavior: value as NavbarBehavior,
              })
            }
          />
          <SegmentedControl
            label="Sidebar Style"
            options={SEGMENTED_OPTIONS.sidebar}
            value={preferences.sidebarStyle}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                sidebarStyle: value as SidebarVariants,
              })
            }
          />

          <SegmentedControl
            label="Scale"
            options={SEGMENTED_OPTIONS.scale}
            value={preferences.scale}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                scale: value as BaseSize,
              })
            }
          />

          <SegmentedControl
            label="Radius"
            options={SEGMENTED_OPTIONS.radius}
            value={preferences.radius.value}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                radius: {
                  value: value,
                  name: SEGMENTED_OPTIONS.radius.find(
                    (option) => option.value === value,
                  )?.label as RadiusSize,
                },
              })
            }
          />

          <Button
            variant="outline"
            className="w-full"
            onClick={() => preferencesStore.resetPreferences()}
          >
            Restore Defaults
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}

function SegmentedControl({
  label,
  options,
  value,
  onChange,
}: {
  label: string;
  options: SegmentedOption[];
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <fieldset className="flex flex-col gap-1.5">
      <Label className="w-full">{label}</Label>
      <div className="flex w-full overflow-hidden rounded-md border border-input">
        {options.map((option) => {
          const isActive = value === option.value;
          return (
            <Button
              key={option.value}
              variant="ghost"
              className={cn(
                'h-8 flex-1 rounded-sm border px-1 font-medium shadow-none',
                isActive && 'bg-muted hover:bg-muted',
              )}
              onClick={() => onChange(option.value)}
            >
              {option.icon ? (
                <option.icon className="size-4" />
              ) : (
                <span className="truncate">{option.label}</span>
              )}
            </Button>
          );
        })}
      </div>
    </fieldset>
  );
}

function getThemePresetClass(name: string) {
  return `${name.trim().toLowerCase().replace(/\s+/g, '-')}`;
}
