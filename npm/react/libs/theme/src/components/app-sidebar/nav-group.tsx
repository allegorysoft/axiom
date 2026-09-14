import * as React from 'react';
import { ChevronDown } from 'lucide-react';

import { type NavGroup } from '@axiomframework/react-core';

import {
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from '../ui/sidebar';

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '../ui/collapsible';

import { NavItemNode } from './nav-node';
import { isBranchActive } from './utils';

type GroupProps = {
  group: NavGroup;
  pathname: string;
};

export function NavGroupSection({ group, pathname }: GroupProps) {
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
