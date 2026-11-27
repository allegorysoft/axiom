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
import { AppRouteTitle } from './app-page-title';

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

        <SidebarInset className="md:peer-data-[variant=floating]:mr-[calc(var(--spacing)-1px)] md:peer-data-[variant=inset]:mr-1">
          {layout === 'default' ? (
            <DefaultLayout />
          ) : (
            <AppTabLayout>
              <Header />
            </AppTabLayout>
          )}

          <footer className="mb-2 shrink-0 sticky bottom-0 px-0.5">
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
      <div className="flex-1 m-1 mb-2 gap-3 p-2 border rounded-lg has-data-[slot=data-table]:border-x-0 has-data-[slot=data-table]:border-t-0 has-data-[slot=data-table]:border-transparent has-data-[slot=data-table]:px-0 has-data-[slot=data-table]:pt-0.75">
        <AppRouteTitle />
        <Outlet />
      </div>
    </>
  );
}
