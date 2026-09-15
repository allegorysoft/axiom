'use client';

import * as React from 'react';
import { HomeIcon } from 'lucide-react';

import { useTranslation } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarRail,
} from '../ui/sidebar';

import { NavMain } from './nav-main';
import { NavProjects } from './nav-projects';
// import { NavUser } from './nav-user';

import { DATA, NAV_GROUPS } from './data';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { NavGroupSection } from './nav-group';
import { NavItemNode } from './nav-node';
import { TenantSwitcher } from './tenant-switcher';

function usePathname() {
  return typeof window === 'undefined' ? '' : window.location.pathname;
}

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const t = useTranslation();
  const pathname = usePathname();

  return (
    <Sidebar collapsible="icon" variant="floating" {...props}>
      <SidebarHeader>
        <TenantSwitcher tenants={DATA.tenants} />

        <SidebarMenu className="gap-1"></SidebarMenu>
        <SidebarHeaderSearch />
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent className="flex flex-col gap-2">
            <SidebarMenu>
              <NavItemNode
                item={{
                  title: t('AxiomBase:Home'),
                  url: '/',
                  icon: <HomeIcon />,
                }}
                pathname={pathname}
              />
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        <NavMain items={DATA.navMain} />
        <NavProjects projects={DATA.projects} />

        {NAV_GROUPS.map((group) => (
          <NavGroupSection
            key={group.title}
            group={group}
            pathname={pathname}
          />
        ))}
      </SidebarContent>

      {/* <SidebarFooter>
        <NavUser user={data.user} />
      </SidebarFooter> */}
      <SidebarRail />
    </Sidebar>
  );
}
