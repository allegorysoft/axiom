import {
  isDevMode,
  environmentStore,
  configureCore,
} from '@axiomframework/react-core';
import { configureShared } from '@axiomframework/react-shared';
import { configureOAuth } from '@axiomframework/react-oauth';

export async function loadEnvironment() {
  const environment = isDevMode()
    ? (await import('../environments/environment')).environment
    : (await import('../environments/environment.production')).environment;

  environmentStore.setEnvironment(environment);
}

export function configureApplication() {
  configureCore();
  configureShared();
  configureOAuth();
}
