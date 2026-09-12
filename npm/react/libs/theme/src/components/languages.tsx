import { memo, useCallback } from 'react';
import { Check, Languages as LanguagesIcon } from 'lucide-react';

import { localizerStore, useLocalizer } from '@axiomframework/react-core';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import { Button } from './ui/button';

const languages = [
  { name: 'en', label: 'English' },
  { name: 'tr', label: 'Türkçe' },
];

interface LanguageItemProps {
  lang: string;
  label: string;
  active: boolean;
  onSelect: (lang: string) => void;
}

const LanguageItem = memo(function LanguageItem({
  lang,
  label,
  active,
  onSelect,
}: LanguageItemProps) {
  return (
    <DropdownMenuItem onClick={() => onSelect(lang)}>
      {label}
      {active && <Check />}
    </DropdownMenuItem>
  );
});

export function Languages() {
  const { name } = useLocalizer((s) => s.culture);

  const handleSelect = useCallback((lang: string) => {
    const item = languages.find((l) => l.name === lang);
    if (item)
      localizerStore.setCulture({ name: item.name, displayName: item.label });
  }, []);

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button variant="ghost" size="lg" className="cursor-pointer">
            <LanguagesIcon />
            {languages.find((l) => l.name === name)?.label}
          </Button>
        }
      />

      <DropdownMenuContent align="end">
        {languages.map(({ name: lang, label }) => (
          <LanguageItem
            key={lang}
            lang={lang}
            label={label}
            active={lang === name}
            onSelect={handleSelect}
          />
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
