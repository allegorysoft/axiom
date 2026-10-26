import { lazy } from 'react';

import { UserRound } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import { userActionStore } from '@axiomframework/react-core';
import { settingsDialogHostStore } from '../settings-dialog/host/settings-dialog-host-store';

export function provideProfileUserActions() {
  userActionStore.add({
    title: 'Profile',
    icon: <HugeiconsIcon icon={UserRound} strokeWidth={2} />,
    action: () => {
      settingsDialogHostStore.open('profile');
    },
  });

  settingsDialogHostStore.register(
    'profile',
    lazy(() =>
      import('./profile-settings-dialog').then((m) => ({
        default: m.ProfileSettingsDialog,
      })),
    ),
  );
}
