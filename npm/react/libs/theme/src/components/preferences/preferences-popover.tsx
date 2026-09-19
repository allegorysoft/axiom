import { useEffect, useLayoutEffect } from 'react';
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

import {
  FONT_OPTIONS,
  PRESET_CLASSES,
  PRESET_OPTIONS,
  SEGMENTED_OPTIONS,
  getOption,
  type BaseSize,
  type FontName,
  type NavbarBehavior,
  type PresetName,
  type RadiusValue,
  type SidebarVariants,
} from './options';
import { PreferenceOption } from './preference-option';
import { preferencesStore } from './preferences-store';
import { usePreferences } from './use-preferences';

const GOOGLE_FONTS: Partial<Record<string, string>> = {
  manrope:
    'https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&display=swap',
  geist:
    'https://fonts.googleapis.com/css2?family=Geist:wght@400;500;600;700&display=swap',
};

export function PreferencesPopover() {
  const t = useTranslation();
  const preferences = usePreferences((state) => state.preferences);

  const preset = getOption(PRESET_OPTIONS, preferences.themePreset);
  const font = getOption(FONT_OPTIONS, preferences.font);

  useLayoutEffect(() => {
    const root = document.documentElement;
    const others = PRESET_CLASSES.filter((c) => c !== preferences.themePreset);

    root.classList.remove(...others, preferences.themePreset);

    root.classList.add(preferences.themePreset);
    document.documentElement.dataset.theme = preferences.themePreset;
  }, [preferences.themePreset]);

  useEffect(() => {
    document.documentElement.dataset.font = preferences.font;

    document.head
      .querySelectorAll('link[data-font]')
      .forEach((link) => link.remove());

    const href = GOOGLE_FONTS[preferences.font];
    if (!href) {
      return;
    }

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    link.dataset.font = preferences.font;
    document.head.appendChild(link);
  }, [preferences.font]);

  useLayoutEffect(() => {
    document.documentElement.dataset.scale = preferences.scale;
  }, [preferences.scale]);

  useEffect(() => {
    document.documentElement.dataset.radius = preferences.radius;
  }, [preferences.radius]);
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
          <Label className="flex-col items-stretch gap-1.5">
            {t('AxiomTheme:ThemePreset')}
            <Select
              value={preferences.themePreset}
              onValueChange={(value) => {
                if (value) {
                  preferencesStore.patchPreferences({
                    themePreset: value as PresetName,
                  });
                }
              }}
            >
              <SelectTrigger className="w-full">
                <SelectValue
                  render={() => (
                    <div className="flex items-center gap-2">
                      <span
                        className="w-2 h-2 rounded-full"
                        style={{ backgroundColor: preset?.color }}
                      />
                      {preset?.label}
                    </div>
                  )}
                />
              </SelectTrigger>

              <SelectContent>
                {PRESET_OPTIONS.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    <div className="flex items-center gap-2">
                      <span
                        className="w-2 h-2 rounded-full"
                        style={{ backgroundColor: option.color }}
                      />
                      {option.label}
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Label>

          <Label className="flex-col items-stretch gap-1.5">
            {t('AxiomTheme:Font')}
            <Select
              value={preferences.font}
              onValueChange={(value) => {
                if (value) {
                  preferencesStore.patchPreferences({
                    font: value as FontName,
                  });
                }
              }}
            >
              <SelectTrigger className="w-full">
                <SelectValue render={() => <span>{font?.label}</span>} />
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
            value={preferences.radius}
            onChange={(value) =>
              preferencesStore.patchPreferences({
                radius: value as RadiusValue,
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
