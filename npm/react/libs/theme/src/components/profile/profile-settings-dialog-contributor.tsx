import { lazy, Suspense, useEffect, useState } from 'react';
import {
  type Tab,
  type TabGroup,
  useProfileTabGroups,
} from '@axiomframework/react-core';
import { useSettingsDialog } from '../shared/settings-dialog-provider';

const PROFILE_DIALOG = 'profile';

const UserSettingsDialog = lazy(() =>
  import('./user-settings-dialog').then((m) => ({
    default: m.UserSettingsDialog,
  })),
);

type Selection = { tab: Tab; group: TabGroup } | null;

export function ProfileSettingsDialogContributor() {
  const { activeTab, dialogOpen, closeDialog, triggerRef } = useSettingsDialog();
  const tabs = useProfileTabGroups();

  const [mounted, setMounted] = useState(false);
  const [selection, setSelection] = useState<Selection>(null);

  const active = activeTab === PROFILE_DIALOG;

  useEffect(() => {
    if (active && dialogOpen) {setMounted(true);}
  }, [active, dialogOpen]);

  useEffect(() => {
    if (active && dialogOpen) setSelection(null);
  }, [active, dialogOpen]);

  const defaultGroup = tabs.find((g) => g.children?.length) ?? tabs[0];
  const group = selection?.group ?? defaultGroup;
  const tab = selection?.tab ?? defaultGroup?.children?.[0];
  const ready = Boolean(group && tab);

  if (!mounted || !ready) {
    return null;
  }

  return (
    <Suspense fallback={null}>
      <UserSettingsDialog
        open={active && dialogOpen}
        onOpenChange={(next) => {
          if (!next) closeDialog();
        }}
        tab={tab}
        group={group}
        onTabChange={(nextTab, nextGroup) =>
          setSelection({ tab: nextTab, group: nextGroup })
        }
        finalFocus={triggerRef}
      />
    </Suspense>
  );
}
