import { createStoreHook } from '../store/axiom-store';
import { DEFAULT_MENU_GROUP } from '../menu/menu';
import type { Nav, NavGroup, NavState } from './nav';
import { navStore } from './nav-store';

export const useNavStore = createStoreHook<NavState>(navStore);

export function useNavGroups(): NavGroup[] {
  return useNavStore((state) => state.groups);
}

export function useNavGroup(title: string = DEFAULT_MENU_GROUP): NavGroup | undefined {
  return useNavStore((state) =>
    state.groups.find((group) => group.title === title),
  );
}

const EMPTY_ITEMS: Nav[] = [];
export function useNavItems(title: string = DEFAULT_MENU_GROUP): Nav[] {
  return useNavStore(
    (state) =>
      state.groups.find((group) => group.title === title)?.children ??
      EMPTY_ITEMS,
  );
}
