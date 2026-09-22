import { HugeiconsIcon } from '@hugeicons/react';
import { cn } from 'cn';

import { useTranslation } from '@axiomframework/react-core';

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
  const t = useTranslation();

  return (
    <fieldset className="flex flex-col gap-1.5">
      <Label className="w-full">{label}</Label>

      <div className="flex w-full overflow-hidden rounded-md border border-input p-0.5">
        {options.map((option) => {
          const isActive = value === option.value;
          const Icon = option.icon;

          return (
            <Button
              key={option.value}
              variant="ghost"
              className={cn(
                'h-7 flex-1 rounded-sm border px-1 shadow-none',
                isActive && 'bg-muted hover:bg-muted',
              )}
              onClick={() => onChange(option.value)}
            >
              {Icon ? (
                <HugeiconsIcon icon={Icon} strokeWidth={2} className="size-4" />
              ) : (
                <span className="truncate">{t(option.label)}</span>
              )}
            </Button>
          );
        })}
      </div>
    </fieldset>
  );
}
