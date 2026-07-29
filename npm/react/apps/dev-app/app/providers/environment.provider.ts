import {
  ApplicationInitializer,
  provideInitializers,
  useEnvironmentStore,
} from '@axiomframework/react-core';

export function provideEnvironment() {
  const environmentInitializer: ApplicationInitializer = {
    configure: async () => {
      const environment = import.meta.env.DEV
        ? (await import('../environments/environment')).environment
        : (await import('../environments/environment.production')).environment;
      useEnvironmentStore.getState().setEnvironment(environment);
    },
  };

  provideInitializers(environmentInitializer);
}
