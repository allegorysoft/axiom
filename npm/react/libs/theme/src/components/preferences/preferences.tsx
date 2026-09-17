import { useState } from 'react';
import { Palette, CircleOff, LucideIcon } from 'lucide-react';

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
import { cn } from '@axiomframework/react-theme/lib/utils';

type SegmentedOption = {
  label: string;
  value: string;
  icon?: LucideIcon;
};

const defaults = {
  preset: 'Default',
  font: 'Geist',
  navbar: 'Sticky',
  sidebar: 'Inset',
  radius: 'MD',
  scale: 'MD',
};

const PRESET_OPTIONS = [
  { label: 'Default', value: 'Default' },
  { label: 'Neutral', value: 'Neutral' },
  { label: 'Vibrant', value: 'Vibrant' },
] as const;

const FONT_OPTIONS = [
  { label: 'Geist', value: 'Geist' },
  { label: 'System', value: 'System' },
  { label: 'Inter', value: 'Inter' },
] as const;

const SEGMENTED_OPTIONS: Record<string, SegmentedOption[]> = {
  navbar: [
    { label: 'Sticky', value: 'Sticky' },
    { label: 'Scroll', value: 'Scroll' },
  ],
  sidebar: [
    { label: 'Inset', value: 'Inset' },
    { label: 'Sidebar', value: 'Sidebar' },
    { label: 'Floating', value: 'Floating' },
  ],
  radius: [
    { label: 'None', value: 'None', icon: CircleOff },
    { label: 'SM', value: 'SM' },
    { label: 'MD', value: 'MD' },
    { label: 'LG', value: 'LG' },
  ],
  scale: [
    { label: 'SM', value: 'SM' },
    { label: 'MD', value: 'MD' },
    { label: 'LG', value: 'LG' },
  ],
};

export function Preferences() {
  const t = useTranslation();
  const [preferences, setPreferences] = useState(defaults);

  const updatePreference = (key: keyof typeof defaults, value: string) => {
    setPreferences((current) => ({ ...current, [key]: value }));
  };

  return (
    <Popover>
      <PopoverTrigger
        render={
          <Button variant="outline" size="icon" className="cursor-pointer">
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
            Theme Preset
            <Select
              value={preferences.preset}
              onValueChange={(value) => {
                if (value) updatePreference('preset', value);
              }}
            >
              <SelectTrigger className="w-full" size="sm">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {PRESET_OPTIONS.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </Label>

          <Label className="flex-col items-stretch gap-1.5 text-xs font-semibold">
            Font
            <Select
              value={preferences.font}
              onValueChange={(value) => {
                if (value) updatePreference('font', value);
              }}
            >
              <SelectTrigger className="w-full" size="sm">
                <SelectValue />
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

          <SegmentedControl
            label="Navbar Behavior"
            options={SEGMENTED_OPTIONS.navbar}
            value={preferences.navbar}
            onChange={(value) => updatePreference('navbar', value)}
          />
          <SegmentedControl
            label="Sidebar Style"
            options={SEGMENTED_OPTIONS.sidebar}
            value={preferences.sidebar}
            onChange={(value) => updatePreference('sidebar', value)}
          />
          <SegmentedControl
            label="Radius"
            options={SEGMENTED_OPTIONS.radius}
            value={preferences.radius}
            onChange={(value) => updatePreference('radius', value)}
          />
          <SegmentedControl
            label="Scale"
            options={SEGMENTED_OPTIONS.scale}
            value={preferences.scale}
            onChange={(value) => updatePreference('scale', value)}
          />

          <Button
            variant="outline"
            className="w-full"
            onClick={() => setPreferences(defaults)}
          >
            Restore Defaults
          </Button>
        </div>
      </PopoverContent>
    </Popover>
  );
}

function SegmentedControl({
  label,
  options,
  value,
  onChange,
}: {
  label: string;
  options: SegmentedOption[];
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <fieldset className="flex flex-col gap-1.5">
      <Label className="w-full">{label}</Label>
      <div className="flex w-full overflow-hidden rounded-md border border-input">
        {options.map((option) => {
          const isActive = value === option.value;
          return (
            <Button
              key={option.value}
              variant="ghost"
              className={cn(
                'h-9 flex-1 rounded-sm border last:border-r-0 px-2 font-medium shadow-none',
                isActive && 'bg-muted hover:bg-muted',
              )}
              onClick={() => onChange(option.value)}
            >
              {option.icon ? (
                <option.icon className="size-4" />
              ) : (
                <span className="truncate">{option.label}</span>
              )}
            </Button>
          );
        })}
      </div>
    </fieldset>
  );
}
