import { HugeiconsIcon, IconSvgElement } from '@hugeicons/react';
import { Button } from '../../ui/button';

export function ChoiceGroup<T extends string>({
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
