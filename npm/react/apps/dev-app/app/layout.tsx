import { getCookie } from '@axiomframework/react-core';
import {
  AppSidebar,
  Header,
  SidebarInset,
  SidebarProvider,
} from '@axiomframework/react-theme/components';
import { Outlet } from 'react-router';

export default function Layout() {
  const state = getCookie<boolean>('sidebar_state');
  return (
    <SidebarProvider defaultOpen={state ?? true}>
      <AppSidebar />
      <SidebarInset>
        <Header />
        <div className="flex flex-1 flex-col gap-4 p-4 pt-2">
          <Outlet />
          <div className="grid auto-rows-min gap-4 md:grid-cols-3">
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
          </div>
          <div className="min-h-[100vh] flex-1 rounded-xl bg-muted/50 md:min-h-min"></div>
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}
