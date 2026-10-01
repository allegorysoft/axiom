import { HugeiconsIcon } from '@hugeicons/react';
import { ChevronLeftIcon } from '@hugeicons/core-free-icons';
import { type NavGroup, useTranslation } from '@axiomframework/react-core';
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
import { useSidebarNavigation } from './use-sidebar-navigation';

const PANEL_CLASS =
  'absolute inset-0 overflow-x-hidden overflow-y-auto transition-transform duration-200 ease-out motion-reduce:transition-none group-data-[collapsible=icon]:overflow-hidden';

export function SidebarNavigation({ groups, pathname }: {
  groups: NavGroup[];
  pathname: string;
}) {
  const t = useTranslation();
  const { panels, depth, containerRef, enter, back } = useSidebarNavigation();
  const defaultGroup = groups.find((group) => group.title === 'Default');
  const otherGroups = groups.filter((group) => group.title !== 'Default');

  return (
    <SidebarContent className="relative overflow-hidden">
      <div
        ref={containerRef}
        className="relative min-h-0 flex-1"
        onKeyDown={(event) => {
          if (event.key === 'Escape' && depth && !event.defaultPrevented) {
            event.preventDefault();
            event.stopPropagation();
            back();
          }
        }}
      >
        <div
          className={PANEL_CLASS}
          data-panel-depth={0}
          inert={depth !== 0}
          aria-hidden={depth !== 0}
          style={{ transform: `translateX(${-depth * 100}%)` }}
        >
          {defaultGroup?.items.length ? (
            <SidebarGroup>
              <SidebarGroupContent>
                <SidebarMenu className="gap-0.5">
                  {defaultGroup.items.map((item) => (
                    <NavItemNode key={item.title} item={item} pathname={pathname} onOpenPanel={enter} />
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
          ) : null}
          {otherGroups.map((group) => (
            <NavGroupSection key={group.title} group={group} pathname={pathname} onOpenPanel={enter} />
          ))}
        </div>

        {panels.map((panel, index) => (
          <div
            key={`${index}:${panel.title}`}
            className={`sidebar-navigation-panel ${PANEL_CLASS}`}
            data-panel-depth={index + 1}
            inert={depth !== index + 1}
            aria-hidden={depth !== index + 1}
            style={{ transform: `translateX(${(index + 1 - depth) * 100}%)` }}
          >
            <SidebarGroup>
              <SidebarMenu>
                <SidebarMenuItem>
                  <SidebarMenuButton
                    data-sidebar-back=""
                    onClick={back}
                    aria-label={`Back to ${index > 0 ? t(panels[index - 1].title) : 'Main Menu'}`}
                    tooltip={`Back to ${index > 0 ? t(panels[index - 1].title) : 'Main Menu'}`}
                    className="bg-sidebar-accent/50 text-sidebar-accent-foreground"
                  >
                    <HugeiconsIcon icon={ChevronLeftIcon} strokeWidth={2} />
                    <span className="truncate">Back to {index > 0 ? t(panels[index - 1].title) : 'Main Menu'}</span>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              </SidebarMenu>
            </SidebarGroup>
            <div className="px-2">
              <SidebarSeparator className="mx-0" />
            </div>
            <SidebarGroup>
              <SidebarGroupContent>
                <SidebarMenu className="gap-0.5">
                  {panel.children?.filter((item) => item.childrenDisplay !== 'group').map((item) => (
                    <NavItemNode key={item.title} item={item} pathname={pathname} onOpenPanel={enter} />
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </SidebarGroup>
            {panel.children?.filter((item) => item.childrenDisplay === 'group').map((item) => (
              <NavGroupSection
                key={item.title}
                group={{ title: item.title, isActive: true, items: item.children ?? [] }}
                collapsible={false}
                pathname={pathname}
                onOpenPanel={enter}
              />
            ))}
          </div>
        ))}
      </div>
    </SidebarContent>
  );
}
