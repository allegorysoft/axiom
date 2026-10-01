import { useSyncExternalStore } from 'react';
import { BookTextIcon, HelpSquareIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  type Nav,
  DEFAULT_MENU_GROUP,
  useNavGroups,
} from '@axiomframework/react-core';

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
