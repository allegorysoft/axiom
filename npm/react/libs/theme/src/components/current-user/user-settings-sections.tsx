import { userProfileStore, useUserProfile } from './user-profile-store';
import type { ReactNode } from 'react';
import { HugeiconsIcon, type IconSvgElement } from '@hugeicons/react';
import {
  LanguagesIcon,
  PaletteIcon,
  Layout01Icon,
  UserAccountIcon,
  SquareUserRoundIcon,
  KeyRoundIcon,
  TimeZoneIcon,
  TextFontIcon,
} from '@hugeicons/core-free-icons';
import {
  localizerStore,
  THEME_OPTIONS,
  useLocalizer,
  useTheme,
  useTranslation,
} from '@axiomframework/react-core';

import { Button } from '../ui/button';
import { Separator } from '../ui/separator';
import { preferencesStore } from '../preferences/preferences-store';
import { usePreferences } from '../preferences/use-preferences';
import {
  SEGMENTED_OPTIONS,
  COLOR_THEME_OPTIONS,
  FONT_OPTIONS,
} from '../preferences/options';
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectLabel,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';
import {
  ProfilePhotoSection,
  AccountInfoSection,
  SecuritySection,
} from './account-sections';

function SettingRow({
  title,
  description,
  children,
}: {
  title: string;
  description: string;
  children: ReactNode;
}) {
  return (
    <div className="flex flex-col gap-4 border-b py-6 last:border-0 sm:flex-row sm:items-center sm:justify-between">
      <div className="space-y-1">
        <h3 className="text-sm font-medium">{title}</h3>
        <p className="max-w-sm text-sm text-muted-foreground">{description}</p>
      </div>
      <div className="shrink-0">{children}</div>
    </div>
  );
}

function ChoiceGroup<T extends string>({
  label,
  options,
  value,
  onChange,
}: {
  label: string;
  options: readonly { value: T; label: string; icon?: IconSvgElement }[];
  value: string;
  onChange: (value: T) => void;
}) {
  return (
    <div
      role="group"
      aria-label={label}
      className="flex flex-wrap gap-1 rounded-lg border bg-muted/30 p-1"
    >
      {options.map((option) => (
        <Button
          key={option.value}
          size="sm"
          variant={value === option.value ? 'secondary' : 'ghost'}
          aria-pressed={value === option.value}
          aria-label={option.label}
          onClick={() => onChange(option.value)}
        >
          {option.icon ? (
            <HugeiconsIcon
              icon={option.icon}
              strokeWidth={2}
              className="size-4"
            />
          ) : (
            option.label
          )}
        </Button>
      ))}
    </div>
  );
}

function AppearanceContent({ children }: { children: ReactNode }) {
  return (
    <>
      <div>{children}</div>
      <Separator className="mb-6" />
      <div className="space-y-5">
        <p className="text-sm text-muted-foreground">
          Restore the default color palette, font, layout, interface size and
          corner radius.
        </p>
        <Button onClick={() => preferencesStore.resetPreferences()}>
          Restore Defaults
        </Button>
      </div>
    </>
  );
}

function LayoutSection() {
  const preferences = usePreferences((state) => state.preferences);
  return (
    <AppearanceContent>
      <SettingRow
        title="Navigation Bar"
        description="Keep the navigation bar visible or let it scroll with the page."
      >
        <ChoiceGroup
          label="Navigation Bar"
          options={SEGMENTED_OPTIONS.navbar}
          value={preferences.navbarBehavior}
          onChange={(navbarBehavior) =>
            preferencesStore.patchPreferences({ navbarBehavior })
          }
        />
      </SettingRow>
      <SettingRow
        title="Sidebar Layout"
        description="Choose how the application sidebar sits alongside your content."
      >
        <ChoiceGroup
          label="Sidebar Layout"
          options={SEGMENTED_OPTIONS.sidebar}
          value={preferences.sidebarStyle}
          onChange={(sidebarStyle) =>
            preferencesStore.patchPreferences({ sidebarStyle })
          }
        />
      </SettingRow>
      <SettingRow
        title="Corner Radius"
        description="Choose how rounded interface elements appear."
      >
        <ChoiceGroup
          label="Corner Radius"
          options={SEGMENTED_OPTIONS.radius}
          value={preferences.radius}
          onChange={(radius) => preferencesStore.patchPreferences({ radius })}
        />
      </SettingRow>
      <SettingRow
        title="User Menu Position"
        description="Choose where your user menu appears."
      >
        <ChoiceGroup
          label="User Menu Position"
          options={
            [
              { value: 'sidebar', label: 'Sidebar' },
              { value: 'navbar', label: 'Navbar' },
            ] as const
          }
          value={preferences.userMenuPosition ?? 'navbar'}
          onChange={(userMenuPosition) =>
            preferencesStore.patchPreferences({ userMenuPosition })
          }
        />
      </SettingRow>
    </AppearanceContent>
  );
}

function ThemeSection() {
  const { theme, setTheme } = useTheme();
  const t = useTranslation();
  const preferences = usePreferences((state) => state.preferences);
  const colorTheme = COLOR_THEME_OPTIONS.find(
    (option) => option.value === preferences.colorTheme,
  );
  return (
    <AppearanceContent>
      <SettingRow
        title="Color Theme"
        description="Choose a color palette for your workspace."
      >
        <Select
          items={COLOR_THEME_OPTIONS}
          value={preferences.colorTheme}
          onValueChange={(value) => {
            const option = COLOR_THEME_OPTIONS.find(
              (item) => item.value === value,
            );
            if (option)
              preferencesStore.patchPreferences({ colorTheme: option.value });
          }}
        >
          <SelectTrigger aria-label="Color Theme" className="w-44">
            <SelectValue
              render={() => (
                <span className="flex items-center gap-2">
                  <span
                    aria-hidden="true"
                    className="size-2 shrink-0 rounded-full"
                    style={{ backgroundColor: colorTheme?.color }}
                  />
                  {colorTheme?.label}
                </span>
              )}
            />
          </SelectTrigger>
          <SelectContent>
            {COLOR_THEME_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                <span className="flex items-center gap-2">
                  <span
                    aria-hidden="true"
                    className="size-2 shrink-0 rounded-full"
                    style={{ backgroundColor: option.color }}
                  />
                  {option.label}
                </span>
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </SettingRow>
      <SettingRow
        title="Theme"
        description="Choose a light or dark appearance, or follow your device."
      >
        <ChoiceGroup
          label="Theme"
          options={THEME_OPTIONS.map((option) => ({
            ...option,
            label: t(option.label).replace(/^AxiomTheme:/, ''),
          }))}
          value={theme}
          onChange={setTheme}
        />
      </SettingRow>
    </AppearanceContent>
  );
}

function TypographySection() {
  const preferences = usePreferences((state) => state.preferences);
  return (
    <AppearanceContent>
      <SettingRow
        title="Font"
        description="Choose the typeface used throughout your workspace."
      >
        <Select
          value={preferences.font}
          onValueChange={(value) => {
            const option = FONT_OPTIONS.find((item) => item.value === value);
            if (option)
              preferencesStore.patchPreferences({ font: option.value });
          }}
        >
          <SelectTrigger aria-label="Font" className="w-44">
            <SelectValue
              render={() => (
                <span>
                  {
                    FONT_OPTIONS.find(
                      (option) => option.value === preferences.font,
                    )?.label
                  }
                </span>
              )}
            />
          </SelectTrigger>
          <SelectContent>
            {Array.from(
              new Set(FONT_OPTIONS.map((option) => option.group)),
            ).map((group) => (
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
            ))}
          </SelectContent>
        </Select>
      </SettingRow>
      <SettingRow
        title="Interface Size"
        description="Adjust the size of text and controls for a comfortable view."
      >
        <ChoiceGroup
          label="Interface Size"
          options={SEGMENTED_OPTIONS.scale}
          value={preferences.scale}
          onChange={(scale) => preferencesStore.patchPreferences({ scale })}
        />
      </SettingRow>
    </AppearanceContent>
  );
}

const LANGUAGE_OPTIONS = [
  { value: 'en', label: 'English' },
  { value: 'tr', label: 'Türkçe' },
  { value: 'es', label: 'Español' },
  { value: 'zh', label: '中文' },
  { value: 'de', label: 'Deutsch' },
  { value: 'fr', label: 'Français' },
  { value: 'ja', label: '日本語' },
];

function LanguageSection() {
  const { name } = useLocalizer((state) => state.culture);
  const selectedLanguage = LANGUAGE_OPTIONS.find(
    (language) => language.value === name,
  );
  return (
    <div className="space-y-6">
      <SettingRow
        title="Display Language"
        description="Choose your preferred application language."
      >
        <Select
          items={LANGUAGE_OPTIONS}
          value={name}
          onValueChange={(value) => {
            const language = LANGUAGE_OPTIONS.find(
              (option) => option.value === value,
            );
            if (language)
              localizerStore.setCulture({
                name: language.value,
                displayName: language.label,
              });
          }}
        >
          <SelectTrigger aria-label="Display Language" className="w-56">
            <SelectValue
              render={() => (
                <span className="flex items-center gap-2">
                  {selectedLanguage?.label ?? name}
                </span>
              )}
            />
          </SelectTrigger>
          <SelectContent>
            {LANGUAGE_OPTIONS.map((language) => (
              <SelectItem key={language.value} value={language.value}>
                <span className="flex items-center gap-2">
                  {language.label}
                </span>
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </SettingRow>
      <p className="text-sm text-muted-foreground">
        Some languages are not fully translated yet. Untranslated content uses
        the application's fallback language.
      </p>
    </div>
  );
}

function TimeZoneSection() {
  const timeZone = useUserProfile((state) => state.timeZone);
  const options = [
    ...new Set([timeZone, 'UTC', ...Intl.supportedValuesOf('timeZone')]),
  ].sort();
  return (
    <div className="space-y-6">
      <SettingRow
        title="Time Zone"
        description="Choose your preferred time zone."
      >
        <Select
          value={timeZone}
          items={options.map((value) => ({
            value,
            label: value.replaceAll('_', ' '),
          }))}
          onValueChange={(value) => {
            if (value && options.includes(value))
              userProfileStore.set((state) => ({ ...state, timeZone: value }));
          }}
        >
          <SelectTrigger aria-label="Time Zone" className="w-56">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {options.map((value) => (
              <SelectItem key={value} value={value}>
                {value.replaceAll('_', ' ')}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </SettingRow>
      <p className="text-sm text-muted-foreground">
        Changes are kept for this session. Account sync will be available later.
      </p>
    </div>
  );
}

// Register new sections here to keep navigation and content in sync.
export const USER_SETTINGS_SECTIONS = [
  {
    id: 'profile',
    label: 'Profile Photo',
    group: 'Account',
    description: 'Choose how you appear across your workspace.',
    icon: SquareUserRoundIcon,
    component: ProfilePhotoSection,
  },
  {
    id: 'account-info',
    label: 'Account Details',
    group: 'Account',
    description: 'Manage your personal details and contact information.',
    icon: UserAccountIcon,
    component: AccountInfoSection,
  },
  {
    id: 'security',
    label: 'Password & Security',
    group: 'Account',
    description: 'Manage your password and protect your account.',
    icon: KeyRoundIcon,
    component: SecuritySection,
  },
  {
    id: 'layout',
    label: 'Layout',
    group: 'Appearance',
    description: 'Make your workspace work for you.',
    icon: Layout01Icon,
    component: LayoutSection,
  },
  {
    id: 'appearance',
    label: 'Theme',
    group: 'Appearance',
    description: 'Choose the colors and theme for your workspace.',
    icon: PaletteIcon,
    component: ThemeSection,
  },
  {
    id: 'typography',
    label: 'Typography',
    group: 'Appearance',
    description: 'Choose your font and a comfortable interface size.',
    icon: TextFontIcon,
    component: TypographySection,
  },
  {
    id: 'language',
    label: 'Language',
    group: 'Language & Region',
    description: 'Use the application in your preferred language.',
    icon: LanguagesIcon,
    component: LanguageSection,
  },
  {
    id: 'time-zone',
    label: 'Time Zone',
    group: 'Language & Region',
    description: 'Set your preferred time zone.',
    icon: TimeZoneIcon,
    component: TimeZoneSection,
  },
] as const;

export type UserSettingsSection = (typeof USER_SETTINGS_SECTIONS)[number]['id'];
