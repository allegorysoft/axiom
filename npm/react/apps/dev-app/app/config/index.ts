import {
  isDevMode,
  environmentStore,
  configureCore,
} from '@axiomframework/react-core';
import { configureShared } from '@axiomframework/react-shared';
import { configureOAuth } from '@axiomframework/react-oauth';
import {
  configureTheme,
  initPreferencesSync,
  provideNavItems,
} from '@axiomframework/react-theme/components';

export async function loadEnvironment() {
  const environment = isDevMode()
    ? (await import('../environments/environment')).environment
    : (await import('../environments/environment.production')).environment;

  environmentStore.setEnvironment(environment);
}

export function configureApplication() {
  configureCore({ localization: { remote: { skipProvider: true } } });
  configureShared();
  configureOAuth({ skipDiscovery: true });
  configureTheme();

  initPreferencesSync();

  provideNavItems();
}
