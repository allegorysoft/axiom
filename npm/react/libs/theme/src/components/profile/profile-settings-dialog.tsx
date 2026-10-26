import {
  DEFAULT_MENU_GROUP,
  useProfileTabGroups,
} from '@axiomframework/react-core';

import type { SettingsDialogProps } from '../settings-dialog/models';
import { SettingsDialog } from '../settings-dialog/settings-dialog';

type ProfileSettingsDialogProps = Pick<
  SettingsDialogProps,
  'open' | 'onOpenChange'
>;

export function ProfileSettingsDialog(props: ProfileSettingsDialogProps) {
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
