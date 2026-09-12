'use client';

import * as React from 'react';
import {
  HomeIcon,
  ChevronDown,
  Settings,
  Building2,
  ChevronRight,
  CircleDot,
} from 'lucide-react';

import {
  type NavGroup,
  type Nav,
  useTranslation,
} from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarRail,
} from './ui/sidebar';

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from './ui/collapsible';

import { NavMain } from './nav-main';
import { NavProjects } from './nav-projects';
// import { NavUser } from './nav-user';
import { TeamSwitcher } from './team-switcher';
import { DATA } from './data';

const items: Nav[] = [
  {
    title: 'Tenant Management',
    icon: <Building2 />,
    isActive: false,
    children: [
      {
        title: 'Tenants',
        url: '/tenant-management',
      },
      {
        title: 'Edition parent',
        children: [{ title: 'Editions', url: '/tenant-management/editions' }],
      },
    ],
  },
  {
    title: 'Setting Management',
    icon: <Settings />,
    url: '/setting-management',
  },
];

const groups: NavGroup[] = [
  { title: 'Analytics', isActive: false, items: [] },
  { title: 'Administrator', isActive: false, items },
];

function usePathname() {
  return typeof window === 'undefined' ? '' : window.location.pathname;
}

export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  const t = useTranslation();
  const pathname = usePathname();

  const homeItem: Nav = {
    title: t('AxiomBase:Home'),
    url: '/',
    icon: <HomeIcon />,
  };

  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarHeader>
        <TeamSwitcher teams={DATA.teams} />
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupContent className="flex flex-col gap-2">
            <SidebarMenu>
              <NavItemNode item={homeItem} pathname={pathname} />
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        <NavMain items={DATA.navMain} />
        <NavProjects projects={DATA.projects} />

        {groups.map((group) => (
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

type GroupProps = {
  group: NavGroup;
  pathname: string;
};
function NavGroupSection({ group, pathname }: GroupProps) {
  const initialOpen =
    group.isActive ||
    group.items.some((item) => isBranchActive(item, pathname));

  const [open, setOpen] = React.useState(initialOpen);

  return (
    <Collapsible open={open} onOpenChange={setOpen} className="group/section">
      <SidebarGroup>
        <SidebarGroupLabel
          render={<CollapsibleTrigger />}
          className="uppercase text-muted-foreground/70 cursor-pointer hover:text-muted-foreground"
        >
          {group.title}
          <ChevronDown className="ml-auto transition-transform group-data-open/section:rotate-180" />
        </SidebarGroupLabel>

        <CollapsibleContent>
          <SidebarGroupContent>
            <SidebarMenu className="gap-1">
              {group.items.length ? (
                group.items.map((item) => (
                  <NavItemNode
                    key={item.title}
                    item={item}
                    pathname={pathname}
                  />
                ))
              ) : (
                <SidebarMenuSub>
                  <SidebarMenuSubItem>
                    <SidebarMenuSubButton>
                      <span className="cursor-pointer">No item found</span>
                    </SidebarMenuSubButton>
                  </SidebarMenuSubItem>
                </SidebarMenuSub>
              )}
            </SidebarMenu>
          </SidebarGroupContent>
        </CollapsibleContent>
      </SidebarGroup>
    </Collapsible>
  );
}

type NodeProps = {
  item: Nav;
  pathname: string;
  variant?: 'main' | 'sub';
};
function NavItemNode({ item, pathname, variant = 'main' }: NodeProps) {
  const hasChildren = Boolean(item.children?.length);
  const branchActive = isBranchActive(item, pathname);
  const isSub = variant === 'sub';

  const Item: React.ElementType = isSub ? SidebarMenuSubItem : SidebarMenuItem;
  const Button: React.ElementType = isSub
    ? SidebarMenuSubButton
    : SidebarMenuButton;

  const [open, setOpen] = React.useState(branchActive);

  if (!hasChildren) {
    return (
      <Item>
        <Button
          tooltip={isSub ? undefined : item.title}
          isActive={branchActive}
          render={item.url ? <a href={item.url} /> : undefined}
        >
          {!isSub && (item.icon ?? <CircleDot />)}
          <span className="truncate">{item.title}</span>
        </Button>
      </Item>
    );
  }

  return (
    <Collapsible
      open={open}
      onOpenChange={setOpen}
      className={isSub ? 'group/subitem' : 'group/item'}
      render={<Item />}
    >
      <CollapsibleTrigger
        render={
          <Button tooltip={isSub ? undefined : item.title} className="w-full" />
        }
      >
        {item.icon}
        <span className="truncate cursor-pointer">{item.title}</span>

        {isSub ? (
          <ChevronDown className="ml-auto size-4 shrink-0 transition-transform duration-200 group-data-open/subitem:rotate-180" />
        ) : (
          <ChevronDown className="ml-auto transition-transform group-data-open/item:rotate-180" />
        )}
      </CollapsibleTrigger>

      <CollapsibleContent>
        <SidebarMenuSub className="mr-0 pr-0">
          {item.children!.map((child) => (
            <NavItemNode
              key={child.title}
              item={child}
              pathname={pathname}
              variant="sub"
            />
          ))}
        </SidebarMenuSub>
      </CollapsibleContent>
    </Collapsible>
  );
}

function isBranchActive(item: Nav, pathname: string): boolean {
  if (item.url && item.url === pathname) return true;
  return (
    item.children?.some((child) => isBranchActive(child, pathname)) ?? false
  );
}
