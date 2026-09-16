import './nav-sample';

import { useSyncExternalStore } from 'react';

import { useNavGroups } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
} from '../ui/sidebar';

import { TENANTS } from './data';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { NavGroupSection } from './nav-group';
import { NavItemNode } from './nav-node';
import { TenantSwitcher } from './tenant-switcher';

const DEFAULT_GROUP = 'Default';

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const pathname = useSyncExternalStore(
    subscribe,
    () => window.location.pathname,
    () => '',
  );

  const groups = useNavGroups();

  const index = groups.findIndex((group) => group.title === DEFAULT_GROUP);
  const defaultGroup = index === -1 ? null : groups[index];
  const otherGroups =
    index === -1 ? groups : groups.filter((_, i) => i !== index);

  return (
    <Sidebar collapsible="icon" variant="floating" {...props}>
      <SidebarHeader>
        <TenantSwitcher tenants={TENANTS} />
        <SidebarHeaderSearch />
      </SidebarHeader>

      <SidebarContent>
        {defaultGroup?.items.length ? (
          <SidebarGroup>
            <SidebarGroupContent className="flex flex-col gap-2">
              <SidebarMenu>
                {defaultGroup.items.map((item) => (
                  <NavItemNode
                    key={item.url ?? item.title}
                    item={item}
                    pathname={pathname}
                  />
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ) : null}

        {otherGroups.map((group) => (
          <NavGroupSection
            key={group.title}
            group={group}
            pathname={pathname}
          />
        ))}
      </SidebarContent>
    </Sidebar>
  );
}

function subscribe(callback: () => void) {
  window.addEventListener('popstate', callback);
  return () => window.removeEventListener('popstate', callback);
}
