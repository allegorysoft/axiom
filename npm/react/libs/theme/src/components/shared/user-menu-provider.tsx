import {
  type ReactNode,
  type RefObject,
  createContext,
  lazy,
  Suspense,
  useRef,
  useState,
} from 'react';

import type { UserSettingsSection } from '../profile/models';

import type { Destination } from './models';

const UserSettingsDialog = lazy(() =>
  import('../profile/user-settings-dialog').then((m) => ({
    default: m.UserSettingsDialog,
  })),
);

const SettingsDialog = lazy(() =>
  import('../application-settings/application-settings-dialog').then((m) => ({
    default: m.ApplicationSettingsDialog,
  })),
);

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

      <Suspense fallback={null}>
        {profileOpen && (
          <UserSettingsDialog
            open={profileOpen}
            onOpenChange={setProfileOpen}
            section={section}
            onSectionChange={setSection}
            finalFocus={triggerRef}
          />
        )}

        {settingsOpen && (
          <SettingsDialog
            open={settingsOpen}
            onOpenChange={setSettingsOpen}
            finalFocus={triggerRef}
          />
        )}
      </Suspense>
    </UserMenuContext.Provider>
  );
}
