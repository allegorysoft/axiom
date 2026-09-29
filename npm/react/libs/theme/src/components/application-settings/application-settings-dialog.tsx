import type { ComponentProps } from 'react';

import {
  type Tab,
  type TabGroup,
  DEFAULT_MENU_GROUP,
  useTabGroups,
} from '@axiomframework/react-core';

import type { DialogContent } from '../ui/dialog';
import { SettingsDialog } from '../shared/settings-dialog';

type AppSettingsDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;

  group: TabGroup;
  tab: Tab;
  onTabChange: (tab: Tab, group: TabGroup) => void;

  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};

export function ApplicationSettingsDialog(props: AppSettingsDialogProps) {
  const tabs = useTabGroups();

  return (
    <SettingsDialog
      {...props}
      title="Application Settings"
      description="Manage application settings."
      groups={tabs.filter((f) => f.title !== DEFAULT_MENU_GROUP)}
    />
  );
}
