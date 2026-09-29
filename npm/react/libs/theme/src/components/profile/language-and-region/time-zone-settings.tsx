import { userStore, useUser } from '@axiomframework/react-core';

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '../../ui/select';

import { SettingRow } from '../setting-row';

export default function TimeZoneSection() {
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
