import { createStoreHook } from '../store/axiom-store';
import type { Nav, NavGroup, NavState } from './nav';
import { DEFAULT_NAV_GROUP, navStore } from './nav-store';

export const useNavStore = createStoreHook<NavState>(navStore);

export function useNavGroup(
  title: string = DEFAULT_NAV_GROUP,
): NavGroup | undefined {
  return useNavStore((state) =>
    state.groups.find((group) => group.title === title),
  );
}

const EMPTY_ITEMS: Nav[] = [];
export function useNavItems(title: string = DEFAULT_NAV_GROUP): Nav[] {
  return useNavStore(
    (state) =>
      state.groups.find((group) => group.title === title)?.items ?? EMPTY_ITEMS,
  );
}

export function useNavGroups(): NavGroup[] {
  return useNavStore((state) => state.groups);
}
