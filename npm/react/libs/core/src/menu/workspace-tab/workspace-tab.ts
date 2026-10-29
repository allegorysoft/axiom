import type { ReactNode } from 'react';
import type { AxiomStore } from '../../models/common';

export interface WorkspaceTab {
  icon?: ReactNode;
  url: string;
  title: string;
  active: boolean;
}

export type WorkspaceTabState = {
  tabs: WorkspaceTab[];
};

export type WorkspaceTabStore = AxiomStore<WorkspaceTabState> & {
  find(title: string): WorkspaceTab | undefined;
  add(tab: WorkspaceTab): void;
  patch(title: string, tab: Partial<WorkspaceTab>): void;
  setActive(title: string, active?: boolean): void;
  remove(title: string): void;
};
