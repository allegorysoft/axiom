import {
  environmentStore,
  provideInitializers,
} from '@axiomframework/react-core';
import { oAuthProvider } from './oauth-provider';

type OAuthOptions = {
  skipDiscovery: boolean;
};

export function configureOAuth(options?: OAuthOptions) {
  if (options?.skipDiscovery !== true) {
    provideInitializers({
      configure: () => {
        const environment = environmentStore.get().environment;
        if (environment?.oauth.flow) {
          oAuthProvider(environment.oauth).initialize();
        }
      },
    });
  }
}
