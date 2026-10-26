import { configureProfile } from './profile/config';
import { configureApplicationSettings } from './application-settings/config';

export function configureTheme() {
  configureProfile();
  configureApplicationSettings();
}
