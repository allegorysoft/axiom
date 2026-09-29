import {
  THEME_OPTIONS,
  useTheme,
  useTranslation,
} from '@axiomframework/react-core';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../../ui/select';

import { preferencesStore } from '../../preferences/preferences-store';
import { usePreferences } from '../../preferences/use-preferences';
import { COLOR_THEME_OPTIONS } from '../../preferences/options';

import { SettingRow } from '../setting-row';
import { AppearanceContent } from './appearance-content';
import { ChoiceGroup } from './choice-group';

export default function ThemeSection() {
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
