import { provideTabs as provideAppSettingsTabs } from './application-settings/provider';
import { provideTabs as provideProfileTabs } from './profile/provider';

export function configureTheme() {
  // provideProfileTabs();
  provideAppSettingsTabs();
}
