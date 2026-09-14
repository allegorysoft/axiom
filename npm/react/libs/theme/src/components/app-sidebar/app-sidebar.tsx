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

import { NavMain } from '../nav-main';
import { NavProjects } from '../nav-projects';
// import { NavUser } from './nav-user';
import { TeamSwitcher } from '../team-switcher';

import { DATA, NAV_GROUPS } from './data';
import { NavGroupSection } from './nav-group';
import { NavItemNode } from './nav-node';

function usePathname() {
  return typeof window === 'undefined' ? '' : window.location.pathname;
}

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const t = useTranslation();
  const pathname = usePathname();

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <TeamSwitcher teams={DATA.teams} />
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
