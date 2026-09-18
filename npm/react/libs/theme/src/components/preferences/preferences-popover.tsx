import { useEffect, useLayoutEffect, useRef } from 'react';
import { Palette } from 'lucide-react';

import { useTranslation } from '@axiomframework/react-core';

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
import {
  FONT_OPTIONS,
  getFontLabel,
  PRESET_OPTIONS,
  getPresetColor,
  SEGMENTED_OPTIONS,
} from './options';
import { PreferenceOption } from './preference-option';

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
                      color: getPresetColor(value) || '#000000',
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
                      title: getFontLabel(value) || 'Geist',
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

          <PreferenceOption
            label={t('AxiomTheme:NavbarBehavior')}
            options={SEGMENTED_OPTIONS.navbar}
            value={preferences.navbarBehavior}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                navbarBehavior: value as NavbarBehavior,
              })
            }
          />

          <PreferenceOption
            label={t('AxiomTheme:SidebarStyle')}
            options={SEGMENTED_OPTIONS.sidebar}
            value={preferences.sidebarStyle}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                sidebarStyle: value as SidebarVariants,
              })
            }
          />

          <PreferenceOption
            label={t('AxiomTheme:Scale')}
            options={SEGMENTED_OPTIONS.scale}
            value={preferences.scale}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                scale: value as BaseSize,
              })
            }
          />

          <PreferenceOption
            label={t('AxiomTheme:Radius')}
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
            {t('AxiomTheme:RestoreDefaults')}
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}

function getThemePresetClass(name: string) {
  return `${name.trim().toLowerCase().replace(/\s+/g, '-')}`;
}
