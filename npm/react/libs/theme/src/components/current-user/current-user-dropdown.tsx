import {
  createContext,
  useContext,
  useRef,
  useState,
  type ReactNode,
  type RefObject,
} from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { UnfoldMoreIcon } from '@hugeicons/core-free-icons';
import { getAvatarFallbackText } from '@axiomframework/react-core';
import { useUserProfile } from './user-profile-store';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';
import { Avatar, AvatarFallback, AvatarImage } from '../ui/avatar';
import { SidebarMenuButton, SidebarMenuItem, useSidebar } from '../ui/sidebar';
import { MainContent } from './main-content';
import type { UserSettingsSection } from './user-settings-sections';
import { UserSettingsDialog } from './user-settings-dialog';
import { SettingsDialog } from './settings-dialog';

type UserMenuContextValue = {
  openDialog: (destination: 'profile' | 'settings') => void;
  dialogOpen: boolean;
  triggerRef: RefObject<HTMLButtonElement | null>;
};
const UserMenuContext = createContext<UserMenuContextValue | null>(null);

export function UserMenuProvider({ children }: { children: ReactNode }) {
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [section, setSection] = useState<UserSettingsSection>('profile');
  const triggerRef = useRef<HTMLButtonElement>(null);
  function openDialog(destination: 'profile' | 'settings') {
    setSection('profile');
    setProfileOpen(destination === 'profile');
    setSettingsOpen(destination === 'settings');
  }
  return (
    <UserMenuContext.Provider
      value={{
        openDialog,
        dialogOpen: settingsOpen || profileOpen,
        triggerRef,
      }}
    >
      {children}
      <UserSettingsDialog
        open={profileOpen}
        onOpenChange={setProfileOpen}
        section={section}
        onSectionChange={setSection}
        finalFocus={triggerRef}
      />
      <SettingsDialog
        open={settingsOpen}
        onOpenChange={setSettingsOpen}
        finalFocus={triggerRef}
      />
    </UserMenuContext.Provider>
  );
}

export function CurrentUserDropdown({
  placement = 'navbar',
}: {
  placement?: 'navbar' | 'sidebar';
}) {
  const context = useContext(UserMenuContext);
  const user = useUserProfile((state) => state);
  const { isMobile, state } = useSidebar();
  const [open, setOpen] = useState(false);
  if (!context)
    throw new Error(
      'CurrentUserDropdown must be used within UserMenuProvider.',
    );
  const sidebar = placement === 'sidebar';
  const collapsedSidebar = sidebar && !isMobile && state === 'collapsed';
  const menu = (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger
        render={
          sidebar && !collapsedSidebar ? (
            <SidebarMenuButton
              ref={context.triggerRef}
              size="lg"
              aria-label="User menu"
              className="gap-3 aria-expanded:bg-sidebar-accent"
            />
          ) : (
            <button
              ref={context.triggerRef}
              type="button"
              aria-label="User menu"
              className="cursor-pointer rounded-full outline-none transition-transform active:scale-95"
            />
          )
        }
      >
        <Avatar className="shrink-0">
          <AvatarImage src={user.photo || undefined} alt={user.name} />
          <AvatarFallback>{getAvatarFallbackText(user.name)}</AvatarFallback>
        </Avatar>
        {sidebar && (
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
        )}
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
