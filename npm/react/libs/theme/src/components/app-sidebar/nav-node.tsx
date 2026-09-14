import * as React from 'react';
import { ChevronDown, CircleDot } from 'lucide-react';
import { cn } from 'cn';

import { type Nav } from '@axiomframework/react-core';

import {
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
} from '../ui/sidebar';

import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from '../ui/collapsible';
import { isBranchActive } from './utils';

type NodeProps = {
  item: Nav;
  pathname: string;
  variant?: 'main' | 'sub';
};
export function NavItemNode({ item, pathname, variant = 'main' }: NodeProps) {
  const hasChildren = Boolean(item.children?.length);
  const branchActive = isBranchActive(item, pathname);
  const isSub = variant === 'sub';

  const [open, setOpen] = React.useState(branchActive);

  if (!hasChildren) {
    if (isSub) {
      return (
        <SidebarMenuSubItem>
          <SidebarMenuSubButton
            isActive={branchActive}
            render={item.url ? <a href={item.url} /> : undefined}
          >
            <span className="truncate">{item.title}</span>
          </SidebarMenuSubButton>
        </SidebarMenuSubItem>
      );
    }

    return (
      <SidebarMenuItem>
        <SidebarMenuButton
          tooltip={item.title}
          isActive={branchActive}
          render={item.url ? <a href={item.url} /> : undefined}
        >
          {item.icon ?? <CircleDot />}
          <span className="truncate">{item.title}</span>
        </SidebarMenuButton>
      </SidebarMenuItem>
    );
  }

  return (
    <Collapsible
      open={open}
      onOpenChange={setOpen}
      className={isSub ? 'group/subitem' : 'group/item'}
      render={isSub ? <SidebarMenuSubItem /> : <SidebarMenuItem />}
    >
      <CollapsibleTrigger
        render={
          <SidebarMenuButton
            tooltip={isSub ? undefined : item.title}
            className="w-full"
          />
        }
      >
        {item.icon}
        <span className="truncate cursor-pointer">{item.title}</span>

        <ChevronDown
          className={cn(
            'ml-auto transition-transform',
            !isSub && 'group-data-open/item:rotate-180',
            isSub &&
              'group-data-open/subitem:rotate-180 size-4 shrink-0 duration-200',
          )}
        />
      </CollapsibleTrigger>

      <CollapsibleContent>
        <SidebarMenuSub>
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
