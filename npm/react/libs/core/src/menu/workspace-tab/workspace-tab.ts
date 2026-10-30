import type { ReactNode } from 'react';
import type { AxiomStore } from '../../models/common';

export interface WorkspaceTab {
  icon?: ReactNode;
  url: string;
  title: string;
  active: boolean;
  empty?: boolean;
}

export type WorkspaceTabState = {
  tabs: WorkspaceTab[];
};

export type WorkspaceTabStore = AxiomStore<WorkspaceTabState> & {
  find(url: string): WorkspaceTab | undefined;
  add(tab: WorkspaceTab): void;
  patch(url: string, tab: Partial<WorkspaceTab>): void;
  setActive(url: string, active?: boolean): void;
  remove(url: string): void;
};
