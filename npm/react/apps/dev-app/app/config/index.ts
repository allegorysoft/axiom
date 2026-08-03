import {
  isDevMode,
  type ApplicationInitializer,
  provideInitializers,
  useEnvironmentStore,
} from '@axiomframework/react-core';

export function configureEnvironment() {
  const environmentInitializer: ApplicationInitializer = {
    configure: async () => {
      const environment = isDevMode()
        ? (await import('../environments/environment')).environment
        : (await import('../environments/environment.production')).environment;
      useEnvironmentStore.getState().setEnvironment(environment);
    },
  };

  provideInitializers(environmentInitializer);
}
