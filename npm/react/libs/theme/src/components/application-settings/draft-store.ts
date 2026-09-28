import { createStore, createStoreHook } from '@axiomframework/react-core';
import type { DraftState } from './models';

export const draftStore = createStore<DraftState>({ saved: {} });
