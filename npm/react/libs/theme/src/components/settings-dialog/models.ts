import type { TabGroup } from '@axiomframework/react-core';
import type { ComponentType, LazyExoticComponent } from 'react';

type BaseProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export type DialogComponent = LazyExoticComponent<ComponentType<BaseProps>>;

export interface DialogState {
  activeDialog: string | undefined;
}

export type SettingsDialogProps = BaseProps & {
  title: string;
  description: string;
  groups: readonly TabGroup[];
};
