import { provideProfileTabs } from './profile/tab-provider';
import { provideAppSettingsTabs } from './application-settings/tab-provider';

export function configureTheme() {
  provideProfileTabs();
  provideAppSettingsTabs();
}
