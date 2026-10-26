import { useNavigate } from 'react-router';
import { HugeiconsIcon } from '@hugeicons/react';
import { LogOutIcon as LogOut } from '@hugeicons/core-free-icons';

import {
  getAvatarFallbackText,
  getOrCreateAuthProvider,
  useTranslation,
  useUser,
  useUserActions,
} from '@axiomframework/react-core';

import {
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
} from '../ui/dropdown-menu';
import { Avatar, AvatarFallback, AvatarImage } from '../ui/avatar';

export function UserDropdownActions() {
  const user = useUser();
  const t = useTranslation();
  const navigate = useNavigate();
  const authProvider = getOrCreateAuthProvider().get();
  const userActions = useUserActions();

  function logOut() {
    authProvider.logout(() => navigate('/'));
  }

  return (
    <>
      <DropdownMenuGroup>
        <DropdownMenuLabel className="px-2 py-2">
          <div className="flex items-center gap-3 py-1">
            <Avatar className="size-8">
              <AvatarImage src={user?.photo || undefined} alt={user?.name} />
              <AvatarFallback>
                {getAvatarFallbackText(user?.name)}
              </AvatarFallback>
            </Avatar>

            <div className="min-w-0 flex-1">
              <p className="text-sm font-semibold leading-tight text-foreground">
                {user?.name}
              </p>
              <p className="mt-1 truncate text-xs font-normal text-muted-foreground">
                {user?.email}
              </p>
            </div>
          </div>
        </DropdownMenuLabel>
      </DropdownMenuGroup>

      <DropdownMenuSeparator className="mx-1 my-1" />

      {userActions?.length ? (
        userActions.map((item) => (
          <DropdownMenuItem
            key={item.title}
            className="gap-2 px-2 py-2"
            onClick={() => item.action()}
          >
            {item.icon}
            <span>{item.title}</span>
          </DropdownMenuItem>
        ))
      ) : (
        <DropdownMenuItem
          key="NoItemFound"
          className="gap-2 px-2 py-2"
          disabled
        >
          {t('AxiomBase:NoItemFound')}
        </DropdownMenuItem>
      )}

      <DropdownMenuSeparator className="mx-1 my-1" />

      <DropdownMenuItem
        variant="destructive"
        className="gap-2 px-2 py-2"
        onClick={logOut}
      >
        <HugeiconsIcon icon={LogOut} strokeWidth={2} />
        {t('AxiomAccount:Logout')}
      </DropdownMenuItem>
    </>
  );
}
