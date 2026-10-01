import { localizerStore, useLocalizer } from '@axiomframework/react-core';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../../ui/select';

import { SettingRow } from '../setting-row';
import { LANGUAGES } from './data';

export default function LanguageSection() {
  const { name } = useLocalizer((state) => state.culture);
  const selectedLanguage = LANGUAGES.find(
    (language) => language.value === name,
  );
  return (
    <div className="space-y-6">
      <SettingRow
        title="Display Language"
        description="Choose your preferred application language."
      >
        <Select
          items={LANGUAGES}
          value={name}
          onValueChange={(value) => {
            const language = LANGUAGES.find((option) => option.value === value);
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
            {LANGUAGES.map((language) => (
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
