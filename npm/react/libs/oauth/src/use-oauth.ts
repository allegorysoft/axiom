import { createStoreHook } from '@axiomframework/react-core';
import { oAuthStore } from './oauth-store';

export const useOAuth = createStoreHook(oAuthStore);
