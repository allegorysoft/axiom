import { HugeiconsIcon } from '@hugeicons/react';
import { CheckIcon as Check, LanguagesIcon } from '@hugeicons/core-free-icons';

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
            <HugeiconsIcon icon={LanguagesIcon} strokeWidth={2} />
          </Button>
        }
      />

      <DropdownMenuContent className="pace-y-1 p-2" align="end">
        {languages.map(({ name: lang, label }) => (
          <DropdownMenuItem key={lang} onClick={() => handleSelect(lang)}>
            {label}
            {lang === name && <HugeiconsIcon icon={Check} strokeWidth={2} />}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
