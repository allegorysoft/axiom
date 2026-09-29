import { useState, type ComponentProps } from 'react';
import type { DialogContent } from '../ui/dialog';

import { SettingsDialog } from '../shared/settings-dialog';

import { APPLICATION_SETTINGS_SECTIONS } from './application-settings-sections';

type AppSettingsDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};

export function ApplicationSettingsDialog(props: AppSettingsDialogProps) {
  const [section, setSection] = useState('general');
  return (
    <SettingsDialog
      {...props}
      title="Settings"
      description="Manage application settings."
      sections={APPLICATION_SETTINGS_SECTIONS}
      section={section}
      onSectionChange={setSection}
    />
  );
}
