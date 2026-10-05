import { HugeiconsIcon } from '@hugeicons/react';
import { CheckIcon as Check, LanguagesIcon } from '@hugeicons/core-free-icons';

import { localizerStore, useLocalizer, useTranslation } from '@axiomframework/react-core';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import { Button } from './ui/button';

const languages = [
  { name: 'en', label: 'English' },
  { name: 'tr', label: 'Türkçe' },
];

export function Languages() {
  const { name } = useLocalizer((s) => s.culture);
  const t = useTranslation();

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

      <DropdownMenuContent className="space-y-1 p-2 w-64" align="end">
        <DropdownMenuGroup>
          <DropdownMenuLabel className="px-2 py-2 text-sm text-foreground">
            {t("AxiomBase:Language")}
            <p className="mt-1 font-normal text-muted-foreground">
              {t("AxiomBase:LanguageDescription")}
            </p>
          </DropdownMenuLabel>
        </DropdownMenuGroup>
        
        <DropdownMenuSeparator className="mx-1 my-1" />

        {languages.map(({ name: lang, label }) => (
          <DropdownMenuItem key={lang} className="px-2 py-2" onClick={() => handleSelect(lang)}>
            {label}
            {lang === name && <HugeiconsIcon className="ml-auto" icon={Check} strokeWidth={2} />}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
