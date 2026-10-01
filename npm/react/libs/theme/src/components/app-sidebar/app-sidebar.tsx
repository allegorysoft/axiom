import { useSyncExternalStore } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { BookTextIcon, HelpSquareIcon } from '@hugeicons/core-free-icons';

import { useNavGroups } from '@axiomframework/react-core';

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

      <SidebarNavigation groups={groups} pathname={pathname} />

      <SidebarFooter className="shrink-0">
        <SidebarMenu className="gap-0.5">
          {RESOURCE_LINKS.map(({ label, icon, href }) => (
            <SidebarMenuItem key={label}>
              <SidebarMenuButton
                tooltip={label}
                render={<a href={href} target="_blank" rel="noopener noreferrer" />}
              >
                <HugeiconsIcon icon={icon} strokeWidth={2} />
                <span className="truncate">{label}</span>
              </SidebarMenuButton>
            </SidebarMenuItem>
          ))}
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
