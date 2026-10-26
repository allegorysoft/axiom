import { provideAppSettingsTabs } from './tab-provider';
import { provideAppSettingsUserActions } from './user-actions-provider';

export function configureApplicationSettings() {
  provideAppSettingsTabs();
  provideAppSettingsUserActions();
}
