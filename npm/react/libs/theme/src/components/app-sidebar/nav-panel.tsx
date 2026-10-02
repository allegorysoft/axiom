import { useState } from 'react';
import { ChevronLeftIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import {
  DEFAULT_MENU_GROUP,
  useTranslation,
  type Nav,
  type NavGroup,
} from '@axiomframework/react-core';

import {
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarSeparator,
} from '../ui/sidebar';

import { NavGroupSection } from './nav-group';
import { NavItemNode } from './nav-node';

type NavPanelProps = {
  groups: NavGroup[];
  pathname: string;
};

export function NavPanel({ groups, pathname }: NavPanelProps) {
  const [stack, setStack] = useState<Nav[]>([]);

  const push = (node: Nav) => {
    if (node.mode !== 'switch-panel' || !node.children?.length) return;
    setStack((prev) => [...prev, node]);
  };

  const popTo = (depth: number) => {
    setStack((prev) => (depth >= prev.length ? prev : prev.slice(0, depth)));
  };

  const current = stack[stack.length - 1];

  return (
    <SidebarContent>
      {current ? (
        <PanelView
          stack={stack}
          pathname={pathname}
          onPush={push}
          onPopTo={popTo}
        />
      ) : (
        <RootView groups={groups} pathname={pathname} onPush={push} />
      )}
    </SidebarContent>
  );
}

type RootViewProps = {
  groups: NavGroup[];
  pathname: string;
  onPush: (node: Nav) => void;
};

function RootView({ groups, pathname, onPush }: RootViewProps) {
  const index = groups.findIndex((g) => g.title === DEFAULT_MENU_GROUP);
  const defaultGroup = index === -1 ? null : groups[index];
  const otherGroups =
    index === -1 ? groups : groups.filter((_, i) => i !== index);

  return (
    <>
      {defaultGroup?.children.length ? (
        <SidebarGroup>
          <SidebarGroupContent className="flex flex-col gap-2">
            <SidebarMenu>
              {defaultGroup.children.map((item) => (
                <NavItemNode
                  key={item.url ?? item.title}
                  item={item}
                  pathname={pathname}
                  onSwitchPanel={onPush}
                />
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      ) : null}

      {otherGroups.map((group) => (
        <NavGroupSection
          key={group.title}
          group={group}
          pathname={pathname}
          onSwitchPanel={onPush}
        />
      ))}
    </>
  );
}

type PanelViewProps = {
  stack: Nav[];
  pathname: string;
  onPush: (node: Nav) => void;
  onPopTo: (depth: number) => void;
};
const ROOT_LABEL = 'Main Menu';

function PanelView({ stack, pathname, onPush, onPopTo }: PanelViewProps) {
  const t = useTranslation();
  const current = stack[stack.length - 1];
  const children = current.children ?? [];

  const parentTitle =
    stack.length > 1 ? stack[stack.length - 2].title : ROOT_LABEL;

  return (
    <>
      <SidebarGroup>
        <SidebarMenu>
          <SidebarMenuItem>
            <SidebarMenuButton
              onClick={() => onPopTo(stack.length - 1)}
              aria-label={t(parentTitle)}
              className="bg-sidebar-accent/50 text-sidebar-accent-foreground"
            >
              <HugeiconsIcon
                icon={ChevronLeftIcon}
                strokeWidth={2}
                className="size-4 shrink-0"
              />
              <span className="truncate">{t(parentTitle)}</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarGroup>

      <div className="px-2">
        <SidebarSeparator className="mx-0" />
      </div>

      <SidebarGroup>
        <SidebarGroupContent className="flex flex-col gap-2">
          <SidebarMenu className="gap-1">
            {children.length ? (
              children.map((child) => (
                <NavItemNode
                  key={child.url ?? child.title}
                  item={child}
                  pathname={pathname}
                  onSwitchPanel={onPush}
                />
              ))
            ) : (
              <span className="text-muted-foreground px-2 text-sm">
                No item found
              </span>
            )}
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>
    </>
  );
}
