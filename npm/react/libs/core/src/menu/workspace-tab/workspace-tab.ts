import type { AxiomStore } from '../../models/common';
import type { MenuNode } from '../menu';

type InternalWorkspaceTab = Pick<MenuNode<unknown>, 'icon' | 'title' | 'active'>;
export interface WorkspaceTab extends InternalWorkspaceTab {
  url: string;
  empty?: boolean;
  ephemeral?: boolean;
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
