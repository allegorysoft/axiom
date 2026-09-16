import { Palette } from 'lucide-react';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from '../ui/button';
import {
  Popover,
  PopoverContent,
  PopoverDescription,
  PopoverHeader,
  PopoverTitle,
  PopoverTrigger,
} from '../ui/popover';

export function Preferences() {
  const t = useTranslation();

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
      </PopoverContent>
    </Popover>
  );
}
