import { useSyncExternalStore } from 'react';

import { DEFAULT_MENU_GROUP, useNavGroups } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarFooter,
} from '../ui/sidebar';

import { usePreferences } from '../preferences/use-preferences';
import { CurrentUserDropdown } from '../current-user/current-user-dropdown';

import { TENANTS } from './data';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { NavGroupSection } from './nav-group';
import { NavItemNode } from './nav-node';
import { TenantSwitcher } from './tenant-switcher';

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const pathname = useSyncExternalStore(
    subscribe,
    () => window.location.pathname,
    () => '',
  );
  const preferences = usePreferences((state) => state.preferences);

  const groups = useNavGroups();

  const index = groups.findIndex((group) => group.title === DEFAULT_MENU_GROUP);
  const defaultGroup = index === -1 ? null : groups[index];
  const otherGroups =
    index === -1 ? groups : groups.filter((_, i) => i !== index);

  return (
    <Sidebar collapsible="icon" variant={preferences.sidebarStyle} {...props}>
      <SidebarHeader>
        <TenantSwitcher tenants={TENANTS} />
        <SidebarHeaderSearch />
      </SidebarHeader>

      <SidebarContent>
        {defaultGroup?.children.length ? (
          <SidebarGroup>
            <SidebarGroupContent className="flex flex-col gap-2">
              <SidebarMenu>
                {defaultGroup.children.map((item) => (
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

      {preferences.userMenuPosition === 'sidebar' && (
        <SidebarFooter>
          <SidebarMenu>
            <CurrentUserDropdown placement="sidebar" />
          </SidebarMenu>
        </SidebarFooter>
      )}
    </Sidebar>
  );
}

function subscribe(callback: () => void) {
  window.addEventListener('popstate', callback);
  return () => window.removeEventListener('popstate', callback);
}
