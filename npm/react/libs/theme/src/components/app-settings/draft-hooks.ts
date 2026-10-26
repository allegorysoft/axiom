import { createStoreHook } from '@axiomframework/react-core';
import { draftStore } from './draft-store';

export const useDrafts = createStoreHook(draftStore);
