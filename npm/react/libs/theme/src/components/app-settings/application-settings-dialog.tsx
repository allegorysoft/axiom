import {
  DEFAULT_MENU_GROUP,
  useAppSettingsTabGroups,
} from '@axiomframework/react-core';

import type { SettingsDialogProps } from '../settings-dialog/models';
import { SettingsDialog } from '../settings-dialog/settings-dialog';

type AppSettingsDialogProps = Pick<
  SettingsDialogProps,
  'open' | 'onOpenChange'
>;

export function ApplicationSettingsDialog(props: AppSettingsDialogProps) {
  const tabs = useAppSettingsTabGroups();

  return (
    <SettingsDialog
      {...props}
      title="Application Settings"
      description="Manage application settings."
      groups={tabs.filter((f) => f.title !== DEFAULT_MENU_GROUP)}
    />
  );
}
