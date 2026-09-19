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
import { PreferenceOption } from './preference-option';
import { preferencesStore } from './preferences-store';
import { usePreferences } from './use-preferences';

const PALETTE_URLS = import.meta.glob<string>(
  ['../../styles/*.css', '!../../styles/index.css'],
  { eager: true, query: '?url&no-inline', import: 'default' },
);

const GOOGLE_FONTS: Partial<Record<string, string>> = {
  // Sans Serif
  figtree:
    'https://fonts.googleapis.com/css2?family=Figtree:wght@400;500;600;700;800&display=swap',
  geist:
    'https://fonts.googleapis.com/css2?family=Geist:wght@400;500;600;700;800&display=swap',
  inter:
    'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap',
  manrope:
    'https://fonts.googleapis.com/css2?family=Manrope:wght@400;500;600;700;800&display=swap',
  montserrat:
    'https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700;800&display=swap',
  'plus-jakarta-sans':
    'https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap',
  poppins:
    'https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700;800&display=swap',

  // Serif
  aleo: 'https://fonts.googleapis.com/css2?family=Aleo:wght@400;500;600;700;800&display=swap',
  'noto-serif':
    'https://fonts.googleapis.com/css2?family=Noto+Serif:wght@400;500;600;700;800&display=swap',
  playfair:
    'https://fonts.googleapis.com/css2?family=Playfair+Display:wght@400;500;600;700;800&display=swap',

  // Monospace
  'ibm-plex-mono':
    'https://fonts.googleapis.com/css2?family=IBM+Plex+Mono:wght@400;500;600;700&display=swap',
  'jetbrains-mono':
    'https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700;800&display=swap',
  'source-code-pro':
    'https://fonts.googleapis.com/css2?family=Source+Code+Pro:wght@400;500;600;700;800&display=swap',
  'space-mono':
    'https://fonts.googleapis.com/css2?family=Space+Mono:wght@400;700&display=swap',
};

export function PreferencesPopover() {
  const t = useTranslation();
  const { theme, setTheme } = useTheme();
  const preferences = usePreferences((state) => state.preferences);

  const colorTheme = getOption(COLOR_THEME_OPTIONS, preferences.colorTheme);
  const font = getOption(FONT_OPTIONS, preferences.font);

  useLayoutEffect(() => {
    const root = document.documentElement;
    const others = COLOR_THEME_CLASSES.filter(
      (c) => c !== preferences.colorTheme,
    );

    root.classList.remove(...others, preferences.colorTheme);

    root.classList.add(preferences.colorTheme);
    root.dataset.colorTheme = preferences.colorTheme;

    const href = PALETTE_URLS[`../../styles/${preferences.colorTheme}.css`];
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

    link.dataset.colorTheme = preferences.colorTheme;
    link.href = href;
  }, [preferences.colorTheme]);

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
                {Array.from(
                  new Set(FONT_OPTIONS.map((option) => option.group)),
                ).map((group) => (
                  <SelectGroup key={group}>
                    <SelectLabel>{group}</SelectLabel>
                    {FONT_OPTIONS.filter(
                      (option) => option.group === group,
                    ).map((option) => (
                      <SelectItem key={option.value} value={option.value}>
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                ))}
              </SelectContent>
            </Select>
          </div>

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
