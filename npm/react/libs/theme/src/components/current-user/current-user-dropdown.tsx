import { useState } from 'react';

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';
import { Avatar, AvatarFallback } from '../ui/avatar';
import { ThemeSelect } from '../theme/theme-select';
import { MainContent } from './main-content';
import { getAvatarFallbackText } from '@axiomframework/react-core';

type UserMenuPage = 'main' | 'theme';

export function CurrentUserDropdown() {
  const [open, setOpen] = useState(false);
  const [page, setPage] = useState<UserMenuPage>('main');

  function menuOpenChange(next: boolean) {
    setOpen(next);
    if (next) {
      setPage('main');
    }
  }

  return (
    <DropdownMenu open={open} onOpenChange={menuOpenChange}>
      <DropdownMenuTrigger
        render={
          <button
            type="button"
            className="cursor-pointer rounded-full outline-none transition-transform active:scale-95"
          />
        }
      >
        <Avatar>
          <AvatarFallback>{getAvatarFallbackText('Masum ULU')}</AvatarFallback>
        </Avatar>
      </DropdownMenuTrigger>

      <DropdownMenuContent align="end" className="w-64 space-y-1 p-2">
        {page === 'theme' ? (
          <ThemeSelect onBack={() => setPage('main')} />
        ) : (
          <MainContent onOpenTheme={() => setPage('theme')} />
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
