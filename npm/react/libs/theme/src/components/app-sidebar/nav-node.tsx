import { useState } from 'react';
import { Link } from 'react-router';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  ChevronDownIcon,
  ChevronRightIcon,
  CircleDotIcon,
} from '@hugeicons/core-free-icons';
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
import { isBranchActive } from './nav-utils';

type Target = '_blank' | '_parent' | '_self' | '_top';
type NodeProps = {
  item: Nav;
  pathname: string;
  variant?: 'main' | 'sub';
  parent?: Nav | null;
  target?: Target;
  onSwitchPanel?: (item: Nav) => void;
};
export function NavItemNode({
  item,
  pathname,
  variant = 'main',
  parent = null,
  target = '_self',
  onSwitchPanel,
}: NodeProps) {
  const t = useTranslation();
  const hasChildren = Boolean(item.children?.length);
  const branchActive = isBranchActive(item, pathname);
  const isSub = variant === 'sub';

  const [open, setOpen] = useState(branchActive);

  const isSwitchPanel = Boolean(
    item.mode === 'switch-panel' &&
    (item.children?.length || item.groups?.length) &&
    onSwitchPanel,
  );

  if (!hasChildren && !isSwitchPanel) {
    const href = item.url ? withGroupName(item.url) : undefined;
    const buttonProps = {
      isActive: branchActive,
      render: href ? <Link to={href} target={target} /> : undefined,
    };

    return isSub ? (
      <SidebarMenuSubItem>
        <SidebarMenuSubButton {...buttonProps}>
          <span className="truncate font-medium">{t(item.title)}</span>
        </SidebarMenuSubButton>
      </SidebarMenuSubItem>
    ) : (
      <SidebarMenuItem>
        <SidebarMenuButton tooltip={t(item.title)} {...buttonProps}>
          {item.icon ?? <HugeiconsIcon icon={CircleDotIcon} strokeWidth={2} />}
          <span className="truncate font-medium">{t(item.title)}</span>
        </SidebarMenuButton>
      </SidebarMenuItem>
    );
  }

  if (isSwitchPanel) {
    const buttonProps = {
      isActive: branchActive,
      onClick: () => onSwitchPanel!(item),
    };

    return isSub ? (
      <SidebarMenuSubItem>
        <SidebarMenuSubButton {...buttonProps}>
          <span className="truncate cursor-pointer font-medium">
            {t(item.title)}
          </span>
          <HugeiconsIcon
            icon={ChevronRightIcon}
            strokeWidth={2}
            className="ml-auto size-4 shrink-0"
          />
        </SidebarMenuSubButton>
      </SidebarMenuSubItem>
    ) : (
      <SidebarMenuItem>
        <SidebarMenuButton tooltip={t(item.title)} {...buttonProps}>
          {item.icon}
          <span className="truncate cursor-pointer font-medium">
            {t(item.title)}
          </span>
          <HugeiconsIcon
            icon={ChevronRightIcon}
            strokeWidth={2}
            className="ml-auto size-4 shrink-0"
          />
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
        <span className="truncate cursor-pointer font-medium">
          {t(item.title)}
        </span>

        <HugeiconsIcon
          icon={ChevronDownIcon}
          strokeWidth={2}
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
              onSwitchPanel={onSwitchPanel}
            />
          ))}
        </SidebarMenuSub>
      </CollapsibleContent>
    </Collapsible>
  );
}

function withGroupName(url: string): string {
  const base = import.meta.env.BASE_URL.replace(/\/+$/, '');
  if (!base) {
    return url;
  }

  //TODO: find a better way for external link(s)
  if (url.startsWith('http')) {
    return url;
  }

  if (url === base || url.startsWith(`${base}/`)) {
    return url;
  }

  return url.startsWith('/') ? `${base}${url}` : `${base}/${url}`;
}
