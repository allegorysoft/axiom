import {
  type ReactNode,
  type RefObject,
  createContext,
  useRef,
  useState,
} from 'react';

import type { UserSettingsSection } from './user-settings-sections';
import { UserSettingsDialog } from './user-settings-dialog';
import { SettingsDialog } from './settings-dialog';

type Destination = 'profile' | 'settings';
type UserMenuContextValue = {
  openDialog: (destination: Destination) => void;
  dialogOpen: boolean;
  triggerRef: RefObject<HTMLButtonElement | null>;
};

export const UserMenuContext = createContext<UserMenuContextValue | null>(null);

export function UserMenuProvider({ children }: { children: ReactNode }) {
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [profileOpen, setProfileOpen] = useState(false);
  const [section, setSection] = useState<UserSettingsSection>('profile');
  const triggerRef = useRef<HTMLButtonElement>(null);

  function openDialog(destination: Destination) {
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
