import { useContext, useState } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { UnfoldMoreIcon } from '@hugeicons/core-free-icons';

import { getAvatarFallbackText } from '@axiomframework/react-core';

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';
import { Avatar, AvatarFallback, AvatarImage } from '../ui/avatar';
import { SidebarMenuButton, SidebarMenuItem, useSidebar } from '../ui/sidebar';

import { UserMenuContext } from './user-menu-provider';
import { useUserProfile } from './user-profile-store';
import { MainContent } from './main-content';

type Props = { placement?: 'navbar' | 'sidebar' };
export function CurrentUserDropdown({ placement = 'navbar' }: Props) {
  const context = useContext(UserMenuContext);
  const user = useUserProfile((state) => state);

  const { isMobile } = useSidebar();
  const [open, setOpen] = useState(false);

  if (!context) {
    throw new Error(
      'CurrentUserDropdown must be used within UserMenuProvider.',
    );
  }

  const sidebar = placement === 'sidebar';

  const Content = sidebar && (
    <>
      <div className="grid min-w-0 flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden">
        <span className="truncate font-medium">{user.name}</span>
        <span className="truncate text-xs text-muted-foreground">
          {user.email}
        </span>
      </div>
      <HugeiconsIcon
        icon={UnfoldMoreIcon}
        strokeWidth={2}
        className="ml-auto size-4 shrink-0 group-data-[collapsible=icon]:hidden"
      />
    </>
  );

  const menu = (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger
        render={(triggerProps) => (
          <MenuTrigger placement={placement} {...triggerProps} />
        )}
      >
        <Avatar className="shrink-0">
          <AvatarImage src={user.photo || undefined} alt={user.name} />
          <AvatarFallback>{getAvatarFallbackText(user.name)}</AvatarFallback>
        </Avatar>

        {Content}
      </DropdownMenuTrigger>

      <DropdownMenuContent
        align="end"
        side={sidebar && !isMobile ? 'right' : 'bottom'}
        className="w-64 space-y-1 p-2"
        finalFocus={context.dialogOpen ? false : context.triggerRef}
      >
        <MainContent
          onOpenSettings={(destination) => {
            setOpen(false);
            context.openDialog(destination);
          }}
        />
      </DropdownMenuContent>
    </DropdownMenu>
  );

  return sidebar ? <SidebarMenuItem>{menu}</SidebarMenuItem> : menu;
}

function MenuTrigger({
  placement,
  ...props
}: Pick<Props, 'placement'> & React.ComponentProps<'button'>) {
  const context = useContext(UserMenuContext);
  if (!context) {
    throw new Error(
      'CurrentUserDropdown must be used within UserMenuProvider.',
    );
  }

  const { isMobile, state } = useSidebar();

  const sidebar = placement === 'sidebar';
  const collapsedSidebar = sidebar && !isMobile && state === 'collapsed';

  if (sidebar && !collapsedSidebar) {
    return (
      <SidebarMenuButton
        ref={context.triggerRef}
        size="lg"
        aria-label="User menu"
        className="gap-3 aria-expanded:bg-sidebar-accent"
        {...props}
      />
    );
  }

  return (
    <button
      ref={context.triggerRef}
      type="button"
      aria-label="User menu"
      className="cursor-pointer rounded-full outline-none transition-transform active:scale-95"
      {...props}
    />
  );
}
