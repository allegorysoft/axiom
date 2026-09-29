import {
  Select,
  SelectContent,
  SelectGroup,
  SelectLabel,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../../ui/select';

import { preferencesStore } from '../../preferences/preferences-store';
import { usePreferences } from '../../preferences/use-preferences';
import { SEGMENTED_OPTIONS, FONT_OPTIONS } from '../../preferences/options';

import { SettingRow } from '../setting-row';
import { AppearanceContent } from './appearance-content';
import { ChoiceGroup } from './choice-group';

export default function TypographySection() {
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
