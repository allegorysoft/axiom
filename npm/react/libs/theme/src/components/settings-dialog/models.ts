import type { TabGroup } from '@axiomframework/react-core';

export type SettingsDialogProps = {
  title: string;
  description: string;

  open: boolean;
  onOpenChange: (open: boolean) => void;

  groups: readonly TabGroup[];
};
