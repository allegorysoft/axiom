import type { ComponentProps } from 'react';

import {
  type Tab,
  type TabGroup,
  DEFAULT_MENU_GROUP,
  useProfileTabGroups,
} from '@axiomframework/react-core';

import type { DialogContent } from '../ui/dialog';
import { SettingsDialog } from '../shared/settings-dialog';

type UserSettingsDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;

  group: TabGroup;
  tab: Tab;
  onTabChange: (tab: Tab, group: TabGroup) => void;

  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};

export function UserSettingsDialog(props: UserSettingsDialogProps) {
  const tabs = useProfileTabGroups();

  return (
    <SettingsDialog
      {...props}
      title="AxiomAccount:Profile"
      description="AxiomAccount:ProfileDialogDescription"
      groups={tabs.filter((f) => f.title !== DEFAULT_MENU_GROUP)}
    />
  );
}
