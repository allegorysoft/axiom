import {
  environmentStore,
  getOrCreateAuthProvider,
  provideInitializers,
} from '@axiomframework/react-core';
import { oAuthProvider } from './oauth-provider';

type OAuthOptions = {
  skipDiscovery?: boolean;
};

export function configureOAuth(options?: OAuthOptions): void {
  provideInitializers({
    configure: async () => {
      const oauth = environmentStore.get().environment?.oauth;
      if (!oauth?.flow) {
        return;
      }

      const provider = getOrCreateAuthProvider(() => oAuthProvider);
      provider.provide(oauth);

      if (options?.skipDiscovery) {
        return;
      }

      await provider.get()?.initialize();
    },
  });
}
