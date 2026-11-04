import { type JSX, useState } from 'react';
import { Link } from 'react-router';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  ChevronDownIcon,
  ChevronRightIcon,
  CircleDotIcon,
} from '@hugeicons/core-free-icons';
import { cn } from 'cn';

import {
  type Nav,
  useTranslation,
  isBranchActive,
  isExternalUrl,
  workspaceTabStore,
} from '@axiomframework/react-core';

import { usePreferencesStore } from '../preferences/preferences-hooks';

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

type Target = '_blank' | '_parent' | '_self' | '_top';
type NodeProps = {
  item: Nav;
  pathname: string;
  external?: boolean;
  variant?: 'main' | 'sub';
  parent?: Nav | null;
  target?: Target;
  onSwitchPanel?: (item: Nav) => void;
};
export function NavItemNode({
  item,
  external,
  pathname,
  variant = 'main',
  parent = null,
  target = '_self',
  onSwitchPanel,
}: NodeProps) {
  const t = useTranslation();
  const tabLayout = usePreferencesStore((s) => s.preferences.layout === 'tab');
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
    return renderMenuItem(
      item,
      target,
      branchActive,
      isSub,
      tabLayout,
      external,
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
              external={external}
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

function renderMenuItem(
  item: Nav,
  target: string | undefined,
  branchActive: boolean,
  isSub: boolean,
  tabLayout: boolean,
  external?: boolean,
) {
  const t = useTranslation();

  const url = item.url;
  const isExternal = isExternalUrl(url) && external === true;
  let render: JSX.Element | undefined;

  if (url) {
    render = isExternal ? (
      <a href={url} target={target} />
    ) : (
      <Link
        to={url}
        target={target}
        onClick={(event) => {
          if (
            !tabLayout ||
            isExternalUrl(url) ||
            (target && target !== '_self') ||
            event.button !== 0 ||
            event.ctrlKey ||
            event.metaKey ||
            event.shiftKey ||
            event.altKey
          ) {
            return;
          }

          event.preventDefault();
          if (workspaceTabStore.find(url)) {
            workspaceTabStore.setActive(url);
            if (event.detail === 2) {
              workspaceTabStore.patch(url, { ephemeral: false });
            }
          } else {
            workspaceTabStore.add({
              url,
              title: item.title,
              icon: item.icon,
              active: true,
              ephemeral: event.detail !== 2,
            });
          }
        }}
      />
    );
  }

  const buttonProps = {
    isActive: branchActive,
    render,
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
