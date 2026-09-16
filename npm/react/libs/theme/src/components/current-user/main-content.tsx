import {
  ChevronRight,
  CircleGauge,
  LogOut,
  Palette,
  Settings,
  UserRound,
} from 'lucide-react';

import {
  getAvatarFallbackText,
  useTranslation,
} from '@axiomframework/react-core';

import {
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from '../ui/dropdown-menu';
import { Avatar, AvatarFallback } from '../ui/avatar';

const USER = {
  name: 'Masum ULU',
  email: 'masumulu@allegorysoft.com',
};
const NAV_ITEMS = [
  { title: 'Profile', icon: UserRound },
  { title: 'Settings', icon: Settings },
  { title: 'Usage', icon: CircleGauge },
] as const;
export function MainContent({ onOpenTheme }: { onOpenTheme: () => void }) {
  const t = useTranslation();

  return (
    <>
      <DropdownMenuGroup>
        <DropdownMenuLabel className="px-2 py-2">
          <div className="flex items-center gap-3 py-1">
            <Avatar className="size-8">
              <AvatarFallback>
                {getAvatarFallbackText(USER.name)}
              </AvatarFallback>
            </Avatar>

            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold leading-tight text-foreground">
                {USER.name}
              </p>
              <p className="mt-1 truncate text-xs font-normal text-muted-foreground">
                {USER.email}
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
