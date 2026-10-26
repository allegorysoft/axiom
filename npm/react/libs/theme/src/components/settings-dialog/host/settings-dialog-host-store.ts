import { createStore } from '@axiomframework/react-core';
import type { DialogComponent, DialogState } from '../models';

const initialState: DialogState = {
  activeDialog: undefined,
};

const baseStore = createStore<DialogState>(initialState);
const registry = new Map<string, DialogComponent>();

export const settingsDialogHostStore = Object.assign(baseStore, {
  open(key: string): void {
    if (registry.has(key)) {
      baseStore.set(() => ({ activeDialog: key }));
    }
  },

  close(): void {
    baseStore.set(() => initialState);
  },

  register(key: string, component: DialogComponent): void {
    registry.set(key, component);
  },

  getDialog(key: string): DialogComponent | undefined {
    return registry.get(key);
  },
});