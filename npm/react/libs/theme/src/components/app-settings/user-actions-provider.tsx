import { lazy } from 'react';

import { Settings } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import { userActionStore } from '@axiomframework/react-core';
import { settingsDialogHostStore } from '../settings-dialog/host/settings-dialog-host-store';

export function provideAppSettingsUserActions() {
  userActionStore.add({
    title: 'Settings',
    icon: <HugeiconsIcon icon={Settings} strokeWidth={2} />,
    action: () => {
      settingsDialogHostStore.open('application');
    },
  });

  settingsDialogHostStore.register(
    'application',
    lazy(() =>
      import('./application-settings-dialog').then((m) => ({
        default: m.ApplicationSettingsDialog,
      })),
    ),
  );
}
