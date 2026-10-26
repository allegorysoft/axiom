import { createStoreHook } from '@axiomframework/react-core';
import { settingsDialogHostStore } from './settings-dialog-host-store';

export const useDialog = createStoreHook(settingsDialogHostStore);