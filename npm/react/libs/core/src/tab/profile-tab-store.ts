import { createTabStore } from './tab-store';
import { createTabHooks } from './tab-hooks';

export const profileTabStore = createTabStore();

export const {
  useTabStore: useProfileTabStore,
  useTabGroups: useProfileTabGroups,
  useTabGroup: useProfileTabGroup,
  useTabItems: useProfileTabItems,
} = createTabHooks(profileTabStore);