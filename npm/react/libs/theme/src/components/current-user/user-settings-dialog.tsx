import type { ComponentProps } from 'react';

import { DialogContent } from '../ui/dialog';
import {
  USER_SETTINGS_SECTIONS,
  type UserSettingsSection,
} from './user-settings-sections';
import { SidebarSettingsDialog } from './sidebar-settings-dialog';

export function UserSettingsDialog(props: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  section: UserSettingsSection;
  onSectionChange: (section: UserSettingsSection) => void;
  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
}) {
  return (
    <SidebarSettingsDialog
      {...props}
      title="Profile"
      description="Manage your profile and application preferences."
      sections={USER_SETTINGS_SECTIONS}
      onSectionChange={(id) => {
        const section = USER_SETTINGS_SECTIONS.find((item) => item.id === id);
        if (section) props.onSectionChange(section.id);
      }}
    />
  );
}
