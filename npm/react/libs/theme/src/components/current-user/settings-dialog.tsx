import { useState, type ComponentProps } from 'react';
import { DialogContent } from '../ui/dialog';
import { SidebarSettingsDialog } from './user-settings-dialog';
import { APPLICATION_SETTINGS_SECTIONS } from './application-settings-sections';

type SettingsDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};

export function SettingsDialog(props: SettingsDialogProps) {
  const [section, setSection] = useState('general');
  return (
    <SidebarSettingsDialog
      {...props}
      title="Settings"
      description="Manage application settings."
      sections={APPLICATION_SETTINGS_SECTIONS}
      section={section}
      onSectionChange={setSection}
    />
  );
}
