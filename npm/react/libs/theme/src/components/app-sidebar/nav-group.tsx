import { useState } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { ChevronDownIcon as ChevronDown } from '@hugeicons/core-free-icons';

import { type Nav, type NavGroup, useTranslation } from '@axiomframework/react-core';

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
  collapsible?: boolean;
  onOpenPanel?: (item: Nav, trigger: HTMLButtonElement) => void;
};

export function NavGroupSection({ group, pathname, onOpenPanel, collapsible = true }: GroupProps) {
  const t = useTranslation();
  const initialOpen =
    group.isActive ||
    group.items.some((item) => isBranchActive(item, pathname));

  const [open, setOpen] = useState(initialOpen);

  return (
    <Collapsible open={collapsible ? open : true} onOpenChange={collapsible ? setOpen : undefined} className="group/section">
      <SidebarGroup>
        <SidebarGroupLabel
          render={collapsible ? <CollapsibleTrigger /> : undefined}
          className={collapsible
            ? 'uppercase text-muted-foreground/70 cursor-pointer hover:text-muted-foreground'
            : 'uppercase text-muted-foreground/70'}
        >
          {t(group.title)}
          {collapsible && <HugeiconsIcon icon={ChevronDown} strokeWidth={2} className="ml-auto transition-transform group-data-open/section:rotate-180" />}
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
                    onOpenPanel={onOpenPanel}
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
