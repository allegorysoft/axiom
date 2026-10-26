import { Suspense } from 'react';

import { settingsDialogHostStore } from './settings-dialog-host-store';
import { useDialog } from './use-settings-dialog';

export function SettingsDialogHost() {
  const key = useDialog((state) => state.activeDialog);
  if (!key) {
    return null;
  }

  const ActiveDialog = settingsDialogHostStore.getDialog(key);
  if (!ActiveDialog) {
    return null;
  }

  return (
    <Suspense fallback={null}>
      <ActiveDialog
        open
        onOpenChange={(open) => {
          if (!open) {
            settingsDialogHostStore.close();
          }
        }}
      />
    </Suspense>
  );
}
