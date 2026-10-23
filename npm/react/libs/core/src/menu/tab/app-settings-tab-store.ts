import { createTabStore } from './tab-store';
import { createTabHooks } from './tab-hooks';

export const appSettingsTabStore = createTabStore();

export const {
  useTabStore: useAppSettingsTabStore,
  useTabGroups: useAppSettingsTabGroups,
  useTabGroup: useAppSettingsTabGroup,
  useTabItems: useAppSettingsTabItems,
} = createTabHooks(appSettingsTabStore);
