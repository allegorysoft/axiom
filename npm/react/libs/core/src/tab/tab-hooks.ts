import { createStoreHook } from '../store/axiom-store';
import { DEFAULT_MENU_GROUP } from '../menu/menu';
import type { Tab, TabGroup, TabState } from './tab';
import { tabStore } from './tab-store';

export const useTabStore = createStoreHook<TabState>(tabStore);

export function useTabGroups(): TabGroup[] {
  return useTabStore((state) => state.groups);
}

export function useTabGroup(
  title: string = DEFAULT_MENU_GROUP,
): TabGroup | undefined {
  return useTabStore((state) =>
    state.groups.find((group) => group.title === title),
  );
}

const EMPTY_ITEMS: Tab[] = [];
export function useTabItems(title: string = DEFAULT_MENU_GROUP): Tab[] {
  return useTabStore(
    (state) =>
      state.groups.find((group) => group.title === title)?.children ??
      EMPTY_ITEMS,
  );
}
