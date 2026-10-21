import { useLocation } from 'react-router';
import { BookTextIcon, HelpSquareIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';
import { type Nav, useNavGroups, useUser } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarHeader,
  SidebarMenu,
  SidebarFooter,
} from '../ui/sidebar';

import { usePreferences } from '../preferences/preferences-hooks';
import { CurrentUserDropdown } from '../current-user/current-user-dropdown';

import { TENANTS } from './data';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { TenantSwitcher } from './tenant-switcher';
import { NavPanel } from './nav-panel';
import { NavItemNode } from './nav-node';
import { useHoverExpand } from './use-hover-expand';
import { LoginButton } from '../login-button';

const RESOURCE_LINKS: readonly Nav[] = [
  {
    title: 'AxiomBase:Support',
    icon: <HugeiconsIcon icon={HelpSquareIcon} strokeWidth={2} />,
    url: 'https://discord.gg/vHxVJd9Bx',
  },
  {
    title: 'AxiomBase:Documents',
    icon: <HugeiconsIcon icon={BookTextIcon} strokeWidth={2} />,
    url: 'https://axiomframework.dev/docs',
  },
];

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const { pathname } = useLocation();
  const user = useUser();
  const preferences = usePreferences();
  const groups = useNavGroups();
  const hoverExpand = useHoverExpand();

  return (
    <Sidebar
      collapsible="icon"
      variant={preferences.sidebarStyle}
      onMouseEnter={hoverExpand.onMouseEnter}
      onMouseLeave={hoverExpand.onMouseLeave}
      {...props}
    >
      <SidebarHeader>
        <TenantSwitcher tenants={TENANTS} />
        <SidebarHeaderSearch />
      </SidebarHeader>

      <NavPanel groups={groups} pathname={pathname} />

      <SidebarFooter>
        <SidebarMenu>
          {RESOURCE_LINKS.map((item) => (
            <NavItemNode
              key={item.url ?? item.title}
              item={item}
              pathname={pathname}
              target="_blank"
            />
          ))}
        </SidebarMenu>

        {user !== null && preferences.userMenuPosition === 'sidebar' && (
          <SidebarMenu>
            <CurrentUserDropdown placement="sidebar" />
          </SidebarMenu>
        )}

        {user === null && preferences.userMenuPosition === 'sidebar' && (
          <SidebarMenu>
            <LoginButton placement="sidebar" />
          </SidebarMenu>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
