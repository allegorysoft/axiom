import { lazy } from 'react';
import { useAppSettingsTabGroups } from '@axiomframework/react-core';
import { createSettingsDialogContributor } from '../shared/settings-dialog-contributor';

export const AppSettingsDialogContributor = createSettingsDialogContributor({
  dialogName: 'settings',
  title: 'Settings',
  description: 'Manage application settings',
  Component: lazy(() =>
    import('./application-settings-dialog').then((m) => ({
      default: m.ApplicationSettingsDialog,
    })),
  ),
  useTabs: useAppSettingsTabGroups,
});
