import { HugeiconsIcon } from '@hugeicons/react';
import {
  LogOutIcon as LogOut,
  SettingsIcon as Settings,
  UserRoundIcon as UserRound,
} from '@hugeicons/core-free-icons';

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
import { Avatar, AvatarFallback, AvatarImage } from '../ui/avatar';

import { useUserProfile } from './user-profile-store';

const NAV_ITEMS = [
  { id: 'profile', title: 'Profile', icon: UserRound },
  { id: 'settings', title: 'Settings', icon: Settings },
] as const;
export function MainContent({
  onOpenSettings,
}: {
  onOpenSettings: (section: 'profile' | 'settings') => void;
}) {
  const user = useUserProfile((state) => state);
  const t = useTranslation();

  return (
    <>
      <DropdownMenuGroup>
        <DropdownMenuLabel className="px-2 py-2">
          <div className="flex items-center gap-3 py-1">
            <Avatar className="size-8">
              <AvatarImage src={user.photo || undefined} alt={user.name} />
              <AvatarFallback>
                {getAvatarFallbackText(user.name)}
              </AvatarFallback>
            </Avatar>

            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold leading-tight text-foreground">
                {user.name}
              </p>
              <p className="mt-1 truncate text-xs font-normal text-muted-foreground">
                {user.email}
              </p>
            </div>
          </div>
        </DropdownMenuLabel>
      </DropdownMenuGroup>

      <DropdownMenuSeparator className="mx-1 my-1" />

      {NAV_ITEMS.map((item) => (
        <DropdownMenuItem
          key={item.title}
          className="gap-2 px-2 py-2"
          onClick={() => onOpenSettings(item.id)}
        >
          <HugeiconsIcon icon={item.icon} strokeWidth={2} />
          <span>{item.title}</span>
        </DropdownMenuItem>
      ))}

      <DropdownMenuSeparator className="mx-1 my-1" />

      <DropdownMenuItem variant="destructive" className="gap-2 px-2 py-2">
        <HugeiconsIcon icon={LogOut} strokeWidth={2} />
        {t('AxiomAccount:Logout')}
      </DropdownMenuItem>
    </>
  );
}
