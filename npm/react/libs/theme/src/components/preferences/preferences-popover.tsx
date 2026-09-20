import { useEffect, useLayoutEffect } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { PaletteIcon as Palette } from '@hugeicons/core-free-icons';

import {
  type Theme,
  useTheme,
  useTranslation,
} from '@axiomframework/react-core';

import { Button } from '../ui/button';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
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
  COLOR_THEME_CLASSES,
  COLOR_THEME_OPTIONS,
  SEGMENTED_OPTIONS,
  getOption,
  type BaseSize,
  type FontName,
  type NavbarBehavior,
  type ColorThemeName,
  type RadiusValue,
  type SidebarVariants,
} from './options';
import type { Preferences } from './preferences';
import { PreferenceOption } from './preference-option';
import { preferencesStore } from './preferences-store';
import { usePreferences } from './use-preferences';

const PALETTE_URLS = import.meta.glob<string>(
  ['../../styles/*.css', '!../../styles/index.css'],
  { eager: true, query: '?url&no-inline', import: 'default' },
);

export function PreferencesPopover() {
  const t = useTranslation();
  const { theme, setTheme } = useTheme();
  const preferences = usePreferences((state) => state.preferences);

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
            <HugeiconsIcon icon={Palette} strokeWidth={2} />
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
          <ColorTheme preferences={preferences} />

          <Font preferences={preferences} />

          <PreferenceOption
            label={t('AxiomTheme:Theme')}
            options={SEGMENTED_OPTIONS.theme}
            value={theme}
            onChange={(value) => setTheme(value as Theme)}
          />

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

function ColorTheme({ preferences }: { preferences: Preferences }) {
  const t = useTranslation();
  const colorTheme = getOption(COLOR_THEME_OPTIONS, preferences.colorTheme);

  useColorTheme(preferences.colorTheme);

  return (
    <div className="flex flex-col gap-1">
      <Label>{t('AxiomTheme:ColorTheme')}</Label>
      <Select
        value={preferences.colorTheme}
        onValueChange={(value) => {
          if (value) {
            preferencesStore.patchPreferences({
              colorTheme: value as ColorThemeName,
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
                  style={{ backgroundColor: colorTheme?.color }}
                />
                {colorTheme?.label}
              </div>
            )}
          />
        </SelectTrigger>

        <SelectContent>
          {COLOR_THEME_OPTIONS.map((option) => (
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
    </div>
  );
}

function useColorTheme(colorTheme: ColorThemeName) {
  useLayoutEffect(() => {
    const root = document.documentElement;
    const others = COLOR_THEME_CLASSES.filter((c) => c !== colorTheme);

    root.classList.remove(...others, colorTheme);
    root.classList.add(colorTheme);
    root.dataset.colorTheme = colorTheme;

    const href = PALETTE_URLS[`../../styles/${colorTheme}.css`];
    let link = document.head.querySelector<HTMLLinkElement>(
      'link[data-color-theme]',
    );

    if (!href) {
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
  }, [colorTheme]);
}

function Font({ preferences }: { preferences: Preferences }) {
  const t = useTranslation();
  const font = getOption(FONT_OPTIONS, preferences.font);

  useFont(preferences.font);

  return (
    <div className="flex flex-col gap-1">
      <Label>{t('AxiomTheme:Font')}</Label>
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
          {Array.from(new Set(FONT_OPTIONS.map((option) => option.group))).map(
            (group) => (
              <SelectGroup key={group}>
                <SelectLabel>{group}</SelectLabel>
                {FONT_OPTIONS.filter((option) => option.group === group).map(
                  (option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ),
                )}
              </SelectGroup>
            ),
          )}
        </SelectContent>
      </Select>
    </div>
  );
}

function useFont(font: FontName) {
  useEffect(() => {
    document.documentElement.dataset.font = font;

    document.head
      .querySelectorAll('link[data-font]')
      .forEach((link) => link.remove());

    const href = FONT_OPTIONS.find((f) => f.value === font)?.url;
    if (!href) {
      return;
    }

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    link.dataset.font = font;
    document.head.appendChild(link);
  }, [font]);
}
