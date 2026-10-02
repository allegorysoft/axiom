import { useSyncExternalStore } from 'react';
import { BookTextIcon, HelpSquareIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';
import { type Nav, useNavGroups } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarHeader,
  SidebarMenu,
  SidebarFooter,
} from '../ui/sidebar';

import { usePreferences } from '../preferences/use-preferences';
import { CurrentUserDropdown } from '../current-user/current-user-dropdown';

import { TENANTS } from './data';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { TenantSwitcher } from './tenant-switcher';
import { NavItemNode } from './nav-node';
import { NavPanel } from './nav-panel';

const RESOURCE_LINKS: readonly Nav[] = [
  {
    title: 'AxiomBase:Support',
    icon: <HugeiconsIcon icon={HelpSquareIcon} strokeWidth={2} />,
    url: 'https://discord.gg/vHxVJd9Bx',
  },
  {
    title: 'AxiomBase:Documents',
    icon: <HugeiconsIcon icon={BookTextIcon} strokeWidth={2} />,
    url: 'https://axiomframework.dev/get-started/overview',
  },
];

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const pathname = useSyncExternalStore(
    subscribe,
    () => window.location.pathname,
    () => '',
  );
  const preferences = usePreferences((state) => state.preferences);
  const groups = useNavGroups();

  return (
    <Sidebar collapsible="icon" variant={preferences.sidebarStyle} {...props}>
      <SidebarHeader>
        <TenantSwitcher tenants={TENANTS} />
        <SidebarHeaderSearch />
      </SidebarHeader>

      <NavPanel groups={groups} pathname={pathname} />

      <SidebarFooter>
        <SidebarMenu>
          {RESOURCE_LINKS.map((item) => {
            return (
              <NavItemNode
                key={item.url ?? item.title}
                item={item}
                pathname={pathname}
                target="_blank"
              />
            );
          })}
        </SidebarMenu>

        {preferences.userMenuPosition === 'sidebar' && (
          <SidebarMenu>
            <CurrentUserDropdown placement="sidebar" />
          </SidebarMenu>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}

function subscribe(callback: () => void) {
  window.addEventListener('popstate', callback);
  return () => window.removeEventListener('popstate', callback);
}
