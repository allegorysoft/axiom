import { HugeiconsIcon } from '@hugeicons/react';
import { cn } from 'cn';

import { Button } from '../ui/button';
import { Label } from '../ui/label';

import type { Option } from './options';

type PreferenceOptionProps = {
  label: string;
  options: readonly Option[];
  value: string;
  onChange: (value: string) => void;
};

export function PreferenceOption({
  label,
  options,
  value,
  onChange,
}: PreferenceOptionProps) {
  return (
    <fieldset className="flex flex-col gap-1.5">
      <Label className="w-full">{label}</Label>
      
      <div className="flex w-full overflow-hidden rounded-md border border-input">
        {options.map((option) => {
          const isActive = value === option.value;
          const Icon = option.icon;

          return (
            <Button
              key={option.value}
              variant="ghost"
              className={cn(
                'h-8 flex-1 rounded-sm border px-1 shadow-none',
                isActive && 'bg-muted hover:bg-muted',
              )}
              onClick={() => onChange(option.value)}
            >
              {Icon ? (
                <HugeiconsIcon icon={Icon} strokeWidth={2} className="size-4" />
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
