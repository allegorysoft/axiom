import { useState } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { ChevronDownIcon as ChevronDown } from '@hugeicons/core-free-icons';

import {
  type Nav,
  type NavGroup,
  useTranslation,
} from '@axiomframework/react-core';

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
import { isBranchActive } from './nav-utils';

type GroupProps = {
  group: NavGroup;
  pathname: string;
  onSwitchPanel?: (item: Nav) => void;
};

export function NavGroupSection({
  group,
  pathname,
  onSwitchPanel,
}: GroupProps) {
  const t = useTranslation();
  const isCollapsible = group.mode === 'collapsible';

  const initialOpen =
    group.isActive ||
    group.children.some((item) => isBranchActive(item, pathname));

  const [open, setOpen] = useState(initialOpen);

  const menuItems = (
    <SidebarMenu className="gap-1">
      {group.children.length ? (
        group.children.map((item) => (
          <NavItemNode
            key={item.title}
            item={item}
            pathname={pathname}
            onSwitchPanel={onSwitchPanel}
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
  );

  const groupContent = (
    <SidebarGroup>
      <SidebarGroupLabel
        render={isCollapsible ? <CollapsibleTrigger /> : undefined}
        className="text-muted-foreground/70 cursor-pointer hover:text-muted-foreground text-sm"
      >
        {t(group.title)}
        {isCollapsible && (
          <HugeiconsIcon
            icon={ChevronDown}
            strokeWidth={2}
            className="ml-auto transition-transform group-data-open/section:rotate-180"
          />
        )}
      </SidebarGroupLabel>

      {isCollapsible ? (
        <CollapsibleContent>
          <SidebarGroupContent>{menuItems}</SidebarGroupContent>
        </CollapsibleContent>
      ) : (
        <SidebarGroupContent>{menuItems}</SidebarGroupContent>
      )}
    </SidebarGroup>
  );

  return isCollapsible ? (
    <Collapsible open={open} onOpenChange={setOpen} className="group/section">
      {groupContent}
    </Collapsible>
  ) : (
    groupContent
  );
}
