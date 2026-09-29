import {
  type ReactNode,
  type RefObject,
  createContext,
  lazy,
  Suspense,
  useRef,
  useState,
} from 'react';

import {
  type TabGroup,
  type Tab,
  useTabGroups,
} from '@axiomframework/react-core';

type Destination = 'profile' | 'settings';
const UserSettingsDialog = lazy(() =>
  import('../profile/user-settings-dialog').then((m) => ({
    default: m.UserSettingsDialog,
  })),
);

type SettingsDialogContextValue = {
  openDialog: (destination: Destination) => void;
  dialogOpen: boolean;
  triggerRef: RefObject<HTMLButtonElement | null>;
};

export const SettingsDialogContext =
  createContext<SettingsDialogContextValue | null>(null);

export function SettingsDialogProvider({ children }: { children: ReactNode }) {
  const tabs = useTabGroups();
  const [group, setGroup] = useState<TabGroup>(tabs.at(1)!);
  const [tab, setTab] = useState<Tab>(tabs.at(1)!.children!.at(0)!);
  const triggerRef = useRef<HTMLButtonElement>(null);

  const [profileOpen, setProfileOpen] = useState(false);

  function onTabChange(tab: Tab, activeGroup: TabGroup) {
    setTab(tab);
    setGroup(activeGroup);
  }

  return (
    <SettingsDialogContext.Provider
      value={{
        openDialog: (destination) => setProfileOpen(!profileOpen),
        dialogOpen: profileOpen,
        triggerRef,
      }}
    >
      {children}

      <Suspense fallback={null}>
        {profileOpen && (
          <UserSettingsDialog
            open={profileOpen}
            onOpenChange={setProfileOpen}
            tab={tab}
            group={group}
            onTabChange={onTabChange}
            finalFocus={triggerRef}
          />
        )}
      </Suspense>
    </SettingsDialogContext.Provider>
  );
}
