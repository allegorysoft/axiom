import { LanguagesIcon, TimeZoneIcon } from '@hugeicons/core-free-icons';
import {
  localizerStore,
  useLocalizer,
  userStore,
  useUser,
} from '@axiomframework/react-core';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../ui/select';

import { SettingRow } from './setting-row';

const LANGUAGE_REGION_DATA = {
  languages: [
    { value: 'en', label: 'English' },
    { value: 'tr', label: 'Türkçe' },
    { value: 'es', label: 'Español' },
    { value: 'zh', label: '中文' },
    { value: 'de', label: 'Deutsch' },
    { value: 'fr', label: 'Français' },
    { value: 'ja', label: '日本語' },
  ],
  language: {
    id: 'language',
    label: 'Language',
    group: 'Language & Region',
    description: 'Use the application in your preferred language.',
    icon: LanguagesIcon,
    component: null,
  },
  timeZone: {
    id: 'time-zone',
    label: 'Time Zone',
    group: 'Language & Region',
    description: 'Set your preferred time zone.',
    icon: TimeZoneIcon,
    component: null,
  },
} as const;

export const LANGUAGE_REGION_SECTIONS = {
  language: {
    ...LANGUAGE_REGION_DATA.language,
    component: LanguageSection,
  },
  timeZone: {
    ...LANGUAGE_REGION_DATA.timeZone,
    component: TimeZoneSection,
  },
} as const;

function LanguageSection() {
  const { name } = useLocalizer((state) => state.culture);
  const selectedLanguage = LANGUAGE_REGION_DATA.languages.find(
    (language) => language.value === name,
  );
  return (
    <div className="space-y-6">
      <SettingRow
        title="Display Language"
        description="Choose your preferred application language."
      >
        <Select
          items={LANGUAGE_REGION_DATA.languages}
          value={name}
          onValueChange={(value) => {
            const language = LANGUAGE_REGION_DATA.languages.find(
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
            {LANGUAGE_REGION_DATA.languages.map((language) => (
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
  const timeZone = useUser((state) => state.timeZone);
  const options = [
    ...new Set([timeZone, 'UTC', ...Intl.getCanonicalLocales('timeZone')]),
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
            label: value.replace('_', ' '),
          }))}
          onValueChange={(value) => {
            if (value && options.includes(value))
              userStore.set((state) => ({ ...state, timeZone: value }));
          }}
        >
          <SelectTrigger aria-label="Time Zone" className="w-56">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {options.map((value) => (
              <SelectItem key={value} value={value}>
                {value.replace('_', ' ')}
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
