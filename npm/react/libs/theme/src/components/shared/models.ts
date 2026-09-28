import type { ComponentType, ComponentProps } from 'react';
import type { IconSvgElement } from '@hugeicons/react';
import type { DialogContent } from '../ui/dialog';

export type Destination = 'profile' | 'settings';

export type SettingsSection = {
  id: string;
  label: string;
  group: string;
  description: string;
  icon: IconSvgElement;
  component: ComponentType;
};

export type SettingsDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  section: string;
  onSectionChange: (section: string) => void;
  title: string;
  description: string;
  sections: readonly SettingsSection[];
  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};
