import { createStoreHook } from '../store/axiom-store';
import { DEFAULT_MENU_GROUP } from '../menu/menu';
import type { Tab, TabGroup, TabState, TabStore } from './tab';

const EMPTY_ITEMS: Tab[] = [];

export function createTabHooks(store: TabStore) {
  const useTabStore = createStoreHook<TabState>(store);

  function useTabGroups(): TabGroup[] {
    return useTabStore((state) => state.groups);
  }

  function useTabGroup(
    title: string = DEFAULT_MENU_GROUP,
  ): TabGroup | undefined {
    return useTabStore((state) =>
      state.groups.find((group) => group.title === title),
    );
  }

  function useTabItems(title: string = DEFAULT_MENU_GROUP): Tab[] {
    return useTabStore(
      (state) =>
        state.groups.find((group) => group.title === title)?.children ??
        EMPTY_ITEMS,
    );
  }

  return { useTabStore, useTabGroups, useTabGroup, useTabItems };
}
