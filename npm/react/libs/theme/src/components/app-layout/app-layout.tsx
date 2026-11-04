import { Outlet } from 'react-router';

import { getCookie } from '@axiomframework/react-core';

import { SidebarInset, SidebarProvider } from '../ui/sidebar';
import {
  useColorTheme,
  useFont,
  useScale,
  useRadius,
  useLayout,
} from '../preferences/preferences-hooks';
import { AppSidebar } from '../app-sidebar/app-sidebar';
import { SettingsDialogProvider } from '../settings-dialog/settings-dialog-provider';
import { SettingsDialogHost } from '../settings-dialog/host/settings-dialog-host';

import { AppTabLayout } from './app-tab-layout';
import { Header } from './header';
import { AppBreadcrumb } from './app-breadcrumb';

export function AppLayout() {
  const state = getCookie<boolean>('sidebar_state');
  const layout = useLayout();

  useColorTheme();
  useFont();
  useScale();
  useRadius();

  return (
    <SidebarProvider defaultOpen={state ?? true}>
      <SettingsDialogProvider>
        <AppSidebar />

        <SidebarInset>
          {layout === 'default' ? (
            <DefaultLayout />
          ) : (
            <AppTabLayout>
              <Header />
            </AppTabLayout>
          )}

          <footer className="shrink-0 sticky bottom-0 mb-2">
            <AppBreadcrumb />
          </footer>
        </SidebarInset>

        <SettingsDialogHost />
      </SettingsDialogProvider>
    </SidebarProvider>
  );
}

function DefaultLayout() {
  return (
    <>
      <Header />
      <div className="flex-1 m-1 mb-2 gap-3 p-2 border rounded-lg">
        <Outlet />
      </div>
    </>
  );
}
