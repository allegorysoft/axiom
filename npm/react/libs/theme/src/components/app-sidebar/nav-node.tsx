import { useState } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { ChevronDownIcon as ChevronDown, CircleDotIcon as CircleDot } from '@hugeicons/core-free-icons';
import { cn } from 'cn';

import { useTranslation, type Nav } from '@axiomframework/react-core';

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
  parent?: Nav | null;
};
export function NavItemNode({ item, pathname, variant = 'main', parent = null }: NodeProps) {
  const t = useTranslation();
  const hasChildren = Boolean(item.children?.length);
  const branchActive = isBranchActive(item, pathname);
  const isSub = variant === 'sub';

  const [open, setOpen] = useState(branchActive);

  if (!hasChildren) {
    const buttonProps = {
      isActive: branchActive,
      render: item.url ? <a href={item.url} /> : undefined,
    };

    return isSub ? (
      <SidebarMenuSubItem>
        <SidebarMenuSubButton {...buttonProps}>
          <span className="truncate">{t(item.title)}</span>
        </SidebarMenuSubButton>
      </SidebarMenuSubItem>
    ) : (
      <SidebarMenuItem>
        <SidebarMenuButton tooltip={t(item.title)} {...buttonProps}>
          {item.icon ?? <HugeiconsIcon icon={CircleDot} strokeWidth={2} />}
          <span className="truncate">{t(item.title)}</span>
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
            tooltip={isSub ? undefined : t(item.title)}
            className="w-full"
          />
        }
      >
        {!parent && item.icon}
        <span className="truncate cursor-pointer">{t(item.title)}</span>

        <HugeiconsIcon icon={ChevronDown} strokeWidth={2}
          className={cn(
            'ml-auto transition-transform size-4 shrink-0 duration-200',
            open && 'rotate-180',
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
              parent={item}
              variant="sub"
            />
          ))}
        </SidebarMenuSub>
      </CollapsibleContent>
    </Collapsible>
  );
}
