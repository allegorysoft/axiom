import { lazy } from 'react';
import { useProfileTabGroups } from '@axiomframework/react-core';
import { createSettingsDialogContributor } from '../shared/settings-dialog-contributor';

export const ProfileSettingsDialogContributor = createSettingsDialogContributor(
  {
    dialogName: 'profile',
    title: 'Profile',
    description: 'Manage your profile settings',
    Component: lazy(() =>
      import('./user-settings-dialog').then((m) => ({
        default: m.UserSettingsDialog,
      })),
    ),
    useTabs: useProfileTabGroups,
  },
);
