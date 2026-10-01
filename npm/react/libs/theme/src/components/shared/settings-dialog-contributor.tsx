import { Suspense, useEffect, useState } from 'react';
import type { Tab, TabGroup } from '@axiomframework/react-core';
import { useSettingsDialog } from '../shared/settings-dialog-provider';
import type { SettingsDialogProps } from './models';

type Selection = { tab: Tab; group: TabGroup } | null;

type Props = {
  dialogName: string;
  Component: Tab<SettingsDialogProps>['component'];
  useTabs: () => readonly TabGroup[];
  title: string;
  description: string;
};

export function createSettingsDialogContributor({
  dialogName,
  Component,
  useTabs,
  title,
  description,
}: Props) {
  return function SettingsDialogContributor() {
    const { activeDialog, dialogOpen, closeDialog, triggerRef } = useSettingsDialog();
    const groups = useTabs();

    const [mounted, setMounted] = useState(false);
    const [selection, setSelection] = useState<Selection>(null);

    const active = activeDialog === dialogName;

    useEffect(() => {
      if (active && dialogOpen) {
        setMounted(true);
        setSelection(null);
      }
    }, [active, dialogOpen]);

    const defaultGroup = groups.find((g) => g.children?.length) ?? groups[0];
    const group = selection?.group ?? defaultGroup;
    const tab = selection?.tab ?? defaultGroup?.children?.[0];
    const ready = Boolean(group && tab);

    if (!mounted || !ready) {
      return null;
    }

    function onOpenChange(next: boolean) {
      if (!next) {
        closeDialog();
      }
    }

    return (
      <Suspense fallback={null}>
        <Component
          title={title}
          description={description}
          open={active && dialogOpen}
          onOpenChange={onOpenChange}
          groups={groups}
          group={group}
          tab={tab}
          onTabChange={(tab, group) => setSelection({ tab, group })}
          finalFocus={triggerRef}
        />
      </Suspense>
    );
  };
}
