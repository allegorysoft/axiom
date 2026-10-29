import { createStoreHook } from '../../store/axiom-store';
import type { WorkspaceTab, WorkspaceTabState } from './workspace-tab';
import { workspaceTabStore } from './workspace-tab-store';

export const useWorkspaceTabStore =
  createStoreHook<WorkspaceTabState>(workspaceTabStore);

export function useWorkspaceTabs(): WorkspaceTab[] {
  return useWorkspaceTabStore((state) => state.tabs);
}
