import {
  environmentStore,
  provideInitializers,
} from '@axiomframework/react-core';
import { oAuthProvider } from './oauth-provider';

type OAuthOptions = {
  skipDiscovery?: boolean;
};

export function configureOAuth(options?: OAuthOptions): void {
  if (options?.skipDiscovery) {
    return;
  }

  provideInitializers({
    configure: async () => {
      const oauth = environmentStore.get().environment?.oauth;
      if (!oauth?.flow) {
        return;
      }

      oAuthProvider.provide(oauth);

      const flow = oAuthProvider.get();
      if (flow) {
        await flow.initialize();
      }
    },
  });
}
