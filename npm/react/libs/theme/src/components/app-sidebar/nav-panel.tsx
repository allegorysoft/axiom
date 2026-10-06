import { useEffect, useState } from 'react';
import { LinkBackwardIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import {
  type Nav,
  type NavGroup,
  DEFAULT_MENU_GROUP,
  useTranslation,
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
import { findStack } from './utils';

type NavPanelProps = {
  groups: NavGroup[];
  pathname: string;
};

export function NavPanel({ groups, pathname }: NavPanelProps) {
  const [stack, setStack] = useState<Nav[]>(() => findStack(groups, pathname));

  useEffect(() => {
    setStack(findStack(groups, pathname));
  }, [pathname, groups]);

  const push = (node: Nav) => {
    if (node.mode !== 'switch-panel') return;
    if (!node.children?.length && !node.groups?.length) return;
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
          key={stack.map((n) => n.title).join('/')}
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
const ROOT_LABEL = 'Main Menu';
function RootView({ groups, pathname, onPush }: RootViewProps) {
  const index = groups.findIndex(
    (g) => g.title === DEFAULT_MENU_GROUP && g.mode !== 'collapsible',
  );
  const defaultGroup = index === -1 ? null : groups[index];
  const otherGroups =
    index === -1 ? groups : groups.filter((_, i) => i !== index);

  return (
    <div className="sidebar-navigation-panel">
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
    </div>
  );
}

type PanelViewProps = {
  stack: Nav[];
  pathname: string;
  onPush: (node: Nav) => void;
  onPopTo: (depth: number) => void;
};

function PanelView({ stack, pathname, onPush, onPopTo }: PanelViewProps) {
  const t = useTranslation();
  const current = stack[stack.length - 1];

  const defaultGroup = current.groups?.find(
    (g) => g.title === DEFAULT_MENU_GROUP && g.mode !== 'collapsible',
  );
  const otherGroups = current.groups?.filter((g) => g !== defaultGroup) ?? [];

  const defaultChildren = [
    ...(current.children ?? []),
    ...(defaultGroup?.children ?? []),
  ];
  const isEmpty = defaultChildren.length === 0 && otherGroups.length === 0;

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
              tooltip={'Back to ' + t(parentTitle)}
            >
              <HugeiconsIcon
                icon={LinkBackwardIcon}
                strokeWidth={2}
                className="size-4 shrink-0"
              />
              <span className="truncate">{t(parentTitle)}</span>
            </SidebarMenuButton>
          </SidebarMenuItem>
        </SidebarMenu>
      </SidebarGroup>

      <div className="px-2 mb-1">
        <SidebarSeparator className="mx-0" />
      </div>

      <div className="sidebar-navigation-panel">
        {defaultChildren.length > 0 && (
          <SidebarGroup>
            <SidebarGroupContent className="flex flex-col gap-2">
              <SidebarMenu>
                {defaultChildren.map((item) => (
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
        )}

        {otherGroups.map((group) => (
          <NavGroupSection
            key={group.title}
            group={group}
            pathname={pathname}
            onSwitchPanel={onPush}
          />
        ))}

        {isEmpty && (
          <span className="text-muted-foreground px-2 text-sm">
            No item found
          </span>
        )}
      </div>
    </>
  );
}
