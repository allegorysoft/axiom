import type { ComponentProps } from 'react';
import type { Tab, TabGroup } from '@axiomframework/react-core';

import type { DialogContent } from '../ui/dialog';

export type SettingsDialogProps = {
  title: string;
  description: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;

  groups: readonly TabGroup[];
  group: TabGroup;
  tab: Tab;
  onTabChange: (tab: Tab, group: TabGroup) => void;

  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};

export type DialogContentHeaderProps = {
  title: string;
  active: Tab;
};
