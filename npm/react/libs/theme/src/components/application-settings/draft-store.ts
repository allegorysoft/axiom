import { createStore } from '@axiomframework/react-core';
import type { DraftState } from './shared/models';

export const draftStore = createStore<DraftState>({ saved: {} });
