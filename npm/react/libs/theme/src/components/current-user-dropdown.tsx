import { useState } from 'react';
import {
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  CircleGauge,
  LogOut,
  Monitor,
  Moon,
  Palette,
  Settings,
  Sun,
  UserRound,
} from 'lucide-react';

import {
  type Theme,
  useTheme,
  useTranslation,
} from '@axiomframework/react-core';

import { Button } from './ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from './ui/dropdown-menu';
import { Avatar, AvatarFallback } from './ui/avatar';

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
          <Button
            variant="ghost"
            size="lg"
            className="gap-2 px-2 py-5 cursor-pointer"
          />
        }
      >
        <Avatar>
          <AvatarFallback>MU</AvatarFallback>
        </Avatar>
        <span className="hidden sm:inline">Masum ULU</span>
        <ChevronDown
          className={`transition-transform ${open ? 'rotate-180' : ''}`}
        />
      </DropdownMenuTrigger>

      <DropdownMenuContent
        align="end"
        className="w-64 space-y-1 p-2 [&_[data-slot=dropdown-menu-item]]:gap-2 [&_[data-slot=dropdown-menu-item]]:px-2 [&_[data-slot=dropdown-menu-item]]:py-2"
      >
        {page === 'theme' ? (
          <ThemeContent onBack={() => setPage('main')} />
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

const THEME_OPTIONS = [
  { value: 'light', icon: Sun, label: 'AxiomTheme:Light' },
  { value: 'dark', icon: Moon, label: 'AxiomTheme:Dark' },
  { value: 'system', icon: Monitor, label: 'AxiomTheme:System' },
] as const;
function ThemeContent({ onBack }: { onBack: () => void }) {
  const t = useTranslation();
  const { theme, setTheme } = useTheme();

  return (
    <>
      <DropdownMenuItem
        className="gap-2 px-2 py-2 font-semibold"
        closeOnClick={false}
        onClick={onBack}
        aria-label="Back to user menu"
      >
        <ChevronLeft />
        <span>{t('AxiomTheme:Theme')}</span>
      </DropdownMenuItem>

      <DropdownMenuSeparator className="mx-1 my-1" />

      <DropdownMenuRadioGroup
        value={theme}
        onValueChange={(value) => setTheme(value as Theme)}
        aria-label="Theme"
      >
        {THEME_OPTIONS.map(({ value, icon: Icon, label }) => (
          <DropdownMenuRadioItem
            key={value}
            value={value}
            className="gap-2 py-2"
            closeOnClick={false}
          >
            <Icon />
            <span>{t(label)}</span>
          </DropdownMenuRadioItem>
        ))}
      </DropdownMenuRadioGroup>
    </>
  );
}
