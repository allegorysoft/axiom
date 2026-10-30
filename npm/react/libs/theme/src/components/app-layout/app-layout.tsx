import {
  getCookie,
  useWorkspaceTabs,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { SidebarInset, SidebarProvider } from '../ui/sidebar';
import { Tabs } from '../ui/tabs';

import {
  useColorTheme,
  useFont,
  useScale,
  useRadius,
} from '../preferences/preferences-hooks';
import { AppSidebar } from '../app-sidebar/app-sidebar';
import { SettingsDialogProvider } from '../settings-dialog/settings-dialog-provider';
import { SettingsDialogHost } from '../settings-dialog/host/settings-dialog-host';

import { Header } from './header';
import { AppBreadcrumb } from './app-breadcrumb';
import { WorkspaceTabLayout } from '../workspace-tab/workspace-tab-layout';

export function AppLayout() {
  const state = getCookie<boolean>('sidebar_state');
  const tabs = useWorkspaceTabs();
  const activeTab = tabs.find((tab) => tab.active);

  useColorTheme();
  useFont();
  useScale();
  useRadius();

  return (
    <SidebarProvider defaultOpen={state ?? true}>
      <SettingsDialogProvider>
        <AppSidebar />

        <SidebarInset>
          <Tabs
            value={activeTab?.url ?? tabs[0]?.url ?? null}
            className="mx-1 gap-0"
            onValueChange={(url) => {
              if (typeof url === 'string') {
                workspaceTabStore.setActive(url);
              }
            }}
          >
            <Header />
            <AppBreadcrumb />

            <div className="mb-2 flex flex-1 flex-col gap-3 px-1 py-1">
              <WorkspaceTabLayout />
            </div>
          </Tabs>
        </SidebarInset>

        <SettingsDialogHost />
      </SettingsDialogProvider>
    </SidebarProvider>
  );
}
