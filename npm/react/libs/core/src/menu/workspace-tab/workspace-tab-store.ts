import { createStore } from '../../store/axiom-store';
import type {
  WorkspaceTab,
  WorkspaceTabState,
  WorkspaceTabStore,
} from './workspace-tab';

const initialState: WorkspaceTabState = {
  tabs: [],
};

const baseStore = createStore<WorkspaceTabState>(initialState);

export const workspaceTabStore: WorkspaceTabStore = Object.assign(
  {},
  baseStore,
  {
    find(title: string): WorkspaceTab | undefined {
      return find(title, baseStore.get().tabs);
    },
    add(tab: WorkspaceTab): void {
      const existing = find(tab.title, baseStore.get().tabs);

      if (existing) {
        return;
      }

      baseStore.set((prev) => ({
        tabs: [...prev.tabs, tab],
      }));
    },
    patch(title: string, tab: Partial<WorkspaceTab>): void {
      const existing = find(title, baseStore.get().tabs);

      if (!existing) {
        throw new Error(`navStore.patchGroup: group "${title}" not found`);
      }

      const patched = { ...existing, ...tab };

      baseStore.set((prev) => ({
        tabs: prev.tabs.map((item) => (item === existing ? patched : item)),
      }));
    },
    setActive(title: string, active = true): void {},
    remove(title: string): void {
      if (!find(title, baseStore.get().tabs)) {
        return;
      }

      baseStore.set((prev) => {
        return {
          ...prev,
          tabs: [...prev.tabs.filter((f) => f.title !== title)],
        };
      });
    },
  },
);

workspaceTabStore.setActive = function (title: string, active = true): void {
  const existing = find(title, baseStore.get().tabs);
  if (!existing) {
    return;
  }

  workspaceTabStore.patch(title, { active });
};

function find(title: string, tabs: WorkspaceTab[]) {
  return tabs.find((f) => f.title.toLocaleUpperCase() === title.toUpperCase());
}
