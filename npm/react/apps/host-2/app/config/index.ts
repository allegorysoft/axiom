import { configureCore } from '@axiomframework/react-core';
import {
  configureTheme,
  initPreferencesSync,
  provideNavItems,
} from '@axiomframework/react-theme/components';
import { configureModule1 } from '@axiomframework/react-module-1/config';
import { provideNavItems as provideModule2NavItems } from '@axiomframework/react-module-2';

export function configureApplication() {
  configureCore({ localization: { remote: { skipProvider: true } } });
  configureTheme();
  initPreferencesSync();

  configureModule1();
  provideModule2NavItems();

  provideNavItems();
}
