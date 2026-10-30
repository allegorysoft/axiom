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
    find(url: string): WorkspaceTab | undefined {
      return find(url, baseStore.get().tabs);
    },

    add(tab: WorkspaceTab): void {
      const emptyTab =
        tab.empty && baseStore.get().tabs.find((item) => item.empty);
      if (emptyTab) {
        this.setActive(emptyTab.url);
        return;
      }

      if (this.find(tab.url)) {
        return;
      }

      baseStore.set((prev) => ({
        tabs: [
          ...prev.tabs.map((existing) =>
            tab.active ? { ...existing, active: false } : existing,
          ),
          { ...tab, active: tab.active || prev.tabs.length === 0 },
        ],
      }));
    },

    patch(url: string, changes: Partial<WorkspaceTab>): void {
      baseStore.set((prev) => ({
        tabs: prev.tabs.map((tab) =>
          equals(tab.url, url) ? { ...tab, ...changes } : tab,
        ),
      }));
    },

    setActive(url: string): void {
      if (!this.find(url)) {
        return;
      }

      baseStore.set((prev) => ({
        tabs: prev.tabs.map((tab) => ({
          ...tab,
          active: equals(tab.url, url),
        })),
      }));
    },

    remove(url: string): void {
      baseStore.set((prev) => {
        const index = prev.tabs.findIndex((tab) => equals(tab.url, url));
        const tabs = prev.tabs.filter((tab) => !equals(tab.url, url));
        if (prev.tabs[index]?.active && tabs.length) {
          tabs[Math.min(index, tabs.length - 1)] = {
            ...tabs[Math.min(index, tabs.length - 1)],
            active: true,
          };
        }
        return { tabs };
      });
    },
  },
);

function find(url: string, tabs: WorkspaceTab[]): WorkspaceTab | undefined {
  return tabs.find((tab) => equals(tab.url, url));
}

function equals(a: string, b: string): boolean {
  return a.toLocaleUpperCase() === b.toLocaleUpperCase();
}
