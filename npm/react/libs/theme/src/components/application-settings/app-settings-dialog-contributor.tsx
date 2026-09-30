import { lazy, Suspense, useEffect, useState } from 'react';
import {
  type Tab,
  type TabGroup,
  useAppSettingsTabGroups,
} from '@axiomframework/react-core';
import { useSettingsDialog } from '../shared/settings-dialog-provider';

export const APP_SETTINGS_DIALOG = 'settings';

const ApplicationSettingsDialog = lazy(() =>
  import('./application-settings-dialog').then((m) => ({
    default: m.ApplicationSettingsDialog,
  })),
);

export function AppSettingsDialogContributor() {
  const { activeTab, dialogOpen, closeDialog, triggerRef } =
    useSettingsDialog();
  const tabs = useAppSettingsTabGroups();

  const [mounted, setMounted] = useState(false);
  const [selection, setSelection] = useState<{
    tab: Tab;
    group: TabGroup;
  } | null>(null);

  const active = activeTab === APP_SETTINGS_DIALOG;

  useEffect(() => {
    if (active && dialogOpen) setMounted(true);
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
      <ApplicationSettingsDialog
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
