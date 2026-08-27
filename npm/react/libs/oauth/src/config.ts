import {
  environmentStore,
  getOrSetAuthProvider,
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

      getOrSetAuthProvider(() => oAuthProvider);
      oAuthProvider.provide(oauth);

      const flow = oAuthProvider.get();
      if (flow && !options?.skipDiscovery) {
        await flow.initialize();
      }
    },
  });
}
