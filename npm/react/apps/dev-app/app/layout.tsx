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

        <div className="flex flex-1 flex-col gap-4 p-4 pt-1">
          <Outlet />
        </main>
      </SidebarInset>
    </SidebarProvider>
  );
}
