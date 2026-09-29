import {
  type ReactNode,
  type RefObject,
  createContext,
  lazy,
  Suspense,
  useEffect,
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
  const triggerRef = useRef<HTMLButtonElement>(null);

  const [open, setOpen] = useState(false);
  const [mounted, setMounted] = useState(false);
  const [selection, setSelection] = useState<{
    tab: Tab;
    group: TabGroup;
  } | null>(null);

  useEffect(() => {
    if (open) {
      setMounted(true);
    }
  }, [open]);

  const defaultGroup = tabs.find((g) => g.children?.length) ?? tabs[0];
  const group = selection?.group ?? defaultGroup;
  const tab = selection?.tab ?? defaultGroup?.children?.[0];
  const ready = Boolean(group && tab);

  const value: SettingsDialogContextValue = {
    openDialog: () => {
      setSelection(null);
      setOpen(true);
    },
    dialogOpen: open,
    triggerRef,
  };

  return (
    <SettingsDialogContext.Provider value={value}>
      {children}

      {mounted && ready && (
        <Suspense fallback={null}>
          <UserSettingsDialog
            open={open}
            onOpenChange={setOpen}
            tab={tab}
            group={group}
            onTabChange={(nextTab, nextGroup) =>
              setSelection({ tab: nextTab, group: nextGroup })
            }
            finalFocus={triggerRef}
          />
        </Suspense>
      )}
    </SettingsDialogContext.Provider>
  );
}
