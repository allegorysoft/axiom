import { configureProfile } from './profile/config';
import { configureApplicationSettings } from './app-settings/config';

export function configureTheme() {
  configureProfile();
  configureApplicationSettings();
}
