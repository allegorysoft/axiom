import { Outlet } from 'react-router';
import { getCookie } from '@axiomframework/react-core';
import { SidebarInset, SidebarProvider } from '../ui/sidebar';
import { SettingsDialogProvider } from '../shared/settings-dialog-provider';
import { AppSidebar } from '../app-sidebar/app-sidebar';
import { Header } from './header';
import { ProfileSettingsDialogContributor } from '../profile/profile-settings-dialog-contributor';
import { AppSettingsDialogContributor } from '../application-settings/app-settings-dialog-contributor';

export function AppLayout() {
  const state = getCookie<boolean>('sidebar_state');

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
