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

export function Languages() {
  const { name } = useLocalizer((s) => s.culture);

  const handleSelect = (lang: string) => {
    const item = languages.find((l) => l.name === lang);
    if (item)
      localizerStore.setCulture({ name: item.name, displayName: item.label });
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button variant="outline" size="icon" className="cursor-pointer">
            <LanguagesIcon />
          </Button>
        }
      />

      <DropdownMenuContent align="end">
        {languages.map(({ name: lang, label }) => (
          <DropdownMenuItem key={lang} onClick={() => handleSelect(lang)}>
            {label}
            {lang === name && <Check />}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
