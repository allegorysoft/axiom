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

      if (options?.skipDiscovery) {
        return;
      }

      const flow = oAuthProvider.get();
      if (flow) {
        await flow.initialize();
      }
    },
  });
}
