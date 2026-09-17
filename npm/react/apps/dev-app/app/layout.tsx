import { Outlet } from 'react-router';
import { getCookie } from '@axiomframework/react-core';
import {
  AppSidebar,
  Header,
  SidebarInset,
  SidebarProvider,
} from '@axiomframework/react-theme/components';

export default function Layout() {
  const state = getCookie<boolean>('sidebar_state');

  return (
    <SidebarProvider defaultOpen={state ?? true}>
      <AppSidebar />
      <SidebarInset>
        <Header />

        <main className="flex flex-1 flex-col gap-4 p-4 pt-1">
          <Outlet />
          <div className="grid auto-rows-min gap-4 md:grid-cols-3">
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
          </div>

          <div className="grid auto-rows-min gap-4 md:grid-cols-2">
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
          </div>

          <div className="grid auto-rows-min gap-4 md:grid-cols-3">
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
          </div>

          <div className="grid auto-rows-min gap-4 md:grid-cols-2">
            <div className="aspect-video rounded-xl bg-muted/50" />
            <div className="aspect-video rounded-xl bg-muted/50" />
          </div>
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}
