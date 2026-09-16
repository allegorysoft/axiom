import { useState } from 'react';
import {
  ChevronRight,
  CircleGauge,
  LogOut,
  Palette,
  Settings,
  UserRound,
} from 'lucide-react';

import { useTranslation } from '@axiomframework/react-core';

import { Button } from './ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import { Avatar, AvatarFallback } from './ui/avatar';
import { ThemeSelect } from './theme/theme-select';

type UserMenuPage = 'main' | 'theme';

export function CurrentUserDropdown() {
  const [open, setOpen] = useState(false);
  const [page, setPage] = useState<UserMenuPage>('main');

  return (
    <DropdownMenu
      open={open}
      onOpenChange={(next) => {
        setOpen(next);
        if (next) {
          setPage('main');
        }
      }}
    >
      <DropdownMenuTrigger
        render={
          <Button variant="ghost" size="icon-lg" className="cursor-pointer" />
        }
      >
        <Avatar>
          <AvatarFallback>MU</AvatarFallback>
        </Avatar>
      </DropdownMenuTrigger>

      <DropdownMenuContent
        align="end"
        className="w-64 space-y-1 p-2 [&_[data-slot=dropdown-menu-item]]:gap-2 [&_[data-slot=dropdown-menu-item]]:px-2 [&_[data-slot=dropdown-menu-item]]:py-2"
      >
        {page === 'theme' ? (
          <ThemeSelect onBack={() => setPage('main')} />
        ) : (
          <MainContent onOpenTheme={() => setPage('theme')} />
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

const NAV_ITEMS = [
  { title: 'Profile', icon: UserRound },
  { title: 'Settings', icon: Settings },
  { title: 'Usage', icon: CircleGauge },
] as const;
function MainContent({ onOpenTheme }: { onOpenTheme: () => void }) {
  const t = useTranslation();

  return (
    <>
      <DropdownMenuGroup>
        <DropdownMenuLabel className="px-2 py-2">
          <div className="flex items-center gap-3 py-1">
            <Avatar className="size-8">
              <AvatarFallback>MU</AvatarFallback>
            </Avatar>
            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold leading-tight text-foreground">
                Masum ULU
              </p>
              <p className="mt-1 truncate text-xs font-normal text-muted-foreground">
                masumulu@allegorysoft.com
              </p>
            </div>
          </div>
        </DropdownMenuLabel>
      </DropdownMenuGroup>

      <DropdownMenuSeparator className="mx-1 my-1" />

      {NAV_ITEMS.map((item) => (
        <DropdownMenuItem key={item.title} className="gap-2 px-2 py-2">
          <item.icon />
          <span>{item.title}</span>
          <ChevronRight className="ml-auto" />
        </DropdownMenuItem>
      ))}

      <DropdownMenuItem
        className="gap-2 px-2 py-2"
        closeOnClick={false}
        onClick={onOpenTheme}
      >
        <Palette />
        <span>{t('AxiomTheme:Theme')}</span>
        <ChevronRight className="ml-auto" />
      </DropdownMenuItem>

      <DropdownMenuSeparator className="mx-1 my-1" />

      <DropdownMenuItem variant="destructive" className="gap-2 px-2 py-2">
        <LogOut />
        {t('AxiomAccount:Logout')}
      </DropdownMenuItem>
    </>
  );
}
