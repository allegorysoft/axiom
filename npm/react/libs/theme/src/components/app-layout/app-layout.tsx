import { Outlet } from 'react-router';

import { getCookie } from '@axiomframework/react-core';

import { SidebarInset, SidebarProvider } from '../ui/sidebar';
import { SettingsDialogProvider } from '../shared/settings-dialog-provider';
import { AppSidebar } from '../app-sidebar/app-sidebar';
import { ProfileSettingsDialogContributor } from '../profile/profile-settings-dialog-contributor';
import { AppSettingsDialogContributor } from '../application-settings/app-settings-dialog-contributor';

import { Header } from './header';
import { useColorTheme } from '../preferences/useColorTheme';
import { useFont } from '../preferences/useFont';
import { usePreferences } from '../preferences/usePreferences';
import { useScale } from '../preferences/useScale';
import { useRadius } from '../preferences/useRadius';

export function AppLayout() {
  const state = getCookie<boolean>('sidebar_state');
  const preferences = usePreferences((state) => state.preferences);

  useColorTheme(preferences);
  useFont(preferences);
  useScale(preferences);
  useRadius(preferences);

  return (
    <SidebarProvider defaultOpen={state ?? true}>
      <SettingsDialogProvider>
        <AppSidebar />

        <SidebarInset>
          <Header />

          <div className="flex flex-1 flex-col gap-4 p-4 pt-1">
            <Outlet />
          </div>
        </SidebarInset>

        <ProfileSettingsDialogContributor />
        <AppSettingsDialogContributor />
      </SettingsDialogProvider>
    </SidebarProvider>
  );
}
