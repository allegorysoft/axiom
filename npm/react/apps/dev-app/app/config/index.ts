import {
  isDevMode,
  useEnvironmentStore,
  configureCore,
} from '@axiomframework/react-core';

export async function loadEnvironment() {
  const environment = isDevMode()
    ? (await import('../environments/environment')).environment
    : (await import('../environments/environment.production')).environment;

  useEnvironmentStore.getState().setEnvironment(environment);
}

export function configureApplication() {
  configureCore();
}
