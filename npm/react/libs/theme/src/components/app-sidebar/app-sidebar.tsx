import { useSyncExternalStore } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { BookTextIcon, HelpSquareIcon } from '@hugeicons/core-free-icons';

import { DEFAULT_MENU_GROUP, useNavGroups } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarFooter,
} from '../ui/sidebar';

import { usePreferences } from '../preferences/use-preferences';
import { CurrentUserDropdown } from '../current-user/current-user-dropdown';

import { useSaasPreview } from '@axiomframework/react-theme/components';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { SidebarNavigation } from './sidebar-navigation';
import { TenantSwitcher } from './tenant-switcher';
import { provideNavItems } from './provide-nav-items';

const RESOURCE_LINKS = [
  {
    label: 'Support',
    icon: HelpSquareIcon,
    href: 'https://discord.gg/vHxVJd9Bx',
  },
  {
    label: 'Documents',
    icon: BookTextIcon,
    href: 'https://axiomframework.dev/get-started/overview',
  },
] as const;

provideNavItems();

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const pathname = useSyncExternalStore(
    subscribe,
    () => window.location.pathname,
    () => '',
  );
  const preferences = usePreferences((state) => state.preferences);

  const groups = useNavGroups();
  const tenants = useSaasPreview((state) => state.tenants);
  const editions = useSaasPreview((state) => state.editions);

  const index = groups.findIndex((group) => group.title === DEFAULT_MENU_GROUP);
  const defaultGroup = index === -1 ? null : groups[index];
  const otherGroups =
    index === -1 ? groups : groups.filter((_, i) => i !== index);

  return (
    <Sidebar collapsible="icon" variant={preferences.sidebarStyle} {...props}>
      <SidebarHeader>
        <TenantSwitcher
          tenants={tenants.map((tenant) => ({
            id: tenant.id,
            name: tenant.name,
            edition:
              editions.find((edition) => edition.id === tenant.edition)?.name ??
              tenant.edition,
            logo:
              tenant.logo ??
              tenant.name
                .split(/\s+/)
                .map((word) => word[0])
                .join('')
                .slice(0, 2)
                .toUpperCase(),
          }))}
        />
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
        )}
      </SidebarFooter>
    </Sidebar>
  );
}

function subscribe(callback: () => void) {
  window.addEventListener('popstate', callback);
  return () => window.removeEventListener('popstate', callback);
}
