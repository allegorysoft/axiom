import { useState } from 'react';
import { cn } from 'cn';
import { SearchIcon, ChevronDownIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import {
  type TabGroup,
  getAvatarFallbackText,
  useTranslation,
  useUser,
} from '@axiomframework/react-core';

import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from '../ui/input-group';
import {
  Collapsible,
  CollapsibleTrigger,
  CollapsibleContent,
} from '../ui/collapsible';
import { Avatar, AvatarFallback, AvatarImage } from '../ui/avatar';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from '../ui/sidebar';

import type { SettingsDialogProps } from './models';
import { useSettingsDialog } from './settings-dialog-provider';

type Props = {
  query: string;
  onQueryChange: (query: string) => void;
};

export function SettingsSidebar({
  query,
  onQueryChange,
  groups,
}: Props & Pick<SettingsDialogProps, 'groups'>) {
  const user = useUser();
  const { isMobile } = useSidebar();

  return (
    <Sidebar
      collapsible={isMobile ? 'offcanvas' : 'none'}
      className="h-auto w-full shrink-0 border-b md:h-full md:w-60 md:border-r md:border-b-0"
    >
      <SidebarHeader className="gap-3 p-3">
        <div className="hidden items-center gap-3 md:flex">
          <Avatar className="size-10">
            <AvatarImage src={user?.photo || undefined} alt={user?.name} />
            <AvatarFallback>{getAvatarFallbackText(user?.name)}</AvatarFallback>
          </Avatar>

          <div className="min-w-0">
            <p className="truncate text-sm font-semibold">{user?.name}</p>
            <p className="mt-0.5 truncate text-xs text-muted-foreground">
              Personal account
            </p>
          </div>
        </div>

        <InputGroup>
          <InputGroupInput
            aria-label="Search settings"
            placeholder="Search…"
            value={query}
            onChange={(event) => onQueryChange(event.target.value)}
          />
          <InputGroupAddon>
            <HugeiconsIcon icon={SearchIcon} strokeWidth={2} />
          </InputGroupAddon>
        </InputGroup>
      </SidebarHeader>

      <SidebarContent className="max-h-[40dvh] px-3 pb-3 pt-0 [scrollbar-color:var(--muted-foreground)_transparent] [scrollbar-width:thin] md:max-h-none">
        <SidebarTab key={query} groups={groups} />
      </SidebarContent>
    </Sidebar>
  );
}

function SidebarTab({ groups }: Pick<SettingsDialogProps, 'groups'>) {
  const t = useTranslation();
  const [collapsed, setCollapsed] = useState<Set<string>>(new Set());
  const { setActiveGroup, activeTab, setActiveTab } = useSettingsDialog();

  const setGroupOpen = (groupTitle: string, open: boolean) => {
    setCollapsed((current) => {
      const next = new Set(current);
      if (open) next.delete(groupTitle);
      else next.add(groupTitle);
      return next;
    });
  };

  return (
    <aside aria-label="Settings" className="flex flex-col">
      {groups.map((group) => {
        const isCollapsible = group.mode === 'collapsible';
        const isCollapsed = collapsed.has(group.title);

        const GroupContent = (
          <SidebarGroupContent>
            <SidebarMenu className="gap-1">
              {group.children.map((item, index) => {
                const isActive = item.title === activeTab?.title;

                return (
                  <SidebarMenuItem key={item.title || index}>
                    <SidebarMenuButton
                      type="button"
                      isActive={isActive}
                      aria-current={isActive}
                      onClick={() => {
                        setActiveGroup(group);
                        setActiveTab(item);
                      }}
                      className="h-9 gap-3 px-3 transition-colors"
                    >
                      {item.icon}
                      <span>{t(item.title)}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                );
              })}
            </SidebarMenu>
          </SidebarGroupContent>
        );

        if (!isCollapsible) {
          return (
            <SidebarGroup key={group.title} className="mb-4 w-full p-0">
              <SidebarGroupLabel className="font-medium text-muted-foreground">
                {t(group.title)}
              </SidebarGroupLabel>

              {GroupContent}
            </SidebarGroup>
          );
        }

        return (
          <Collapsible
            key={group.title}
            open={!isCollapsed}
            onOpenChange={(open) => setGroupOpen(group.title, open)}
          >
            <SidebarGroup className="mb-4 w-full p-0">
              <CollapsibleTrigger className="flex h-8 w-full cursor-pointer items-center justify-between rounded-md px-2 text-xs font-medium text-muted-foreground outline-none hover:text-sidebar-foreground focus-visible:ring-2 focus-visible:ring-sidebar-ring">
                {t(group.title)}

                <HugeiconsIcon
                  icon={ChevronDownIcon}
                  strokeWidth={2}
                  className={cn(
                    'size-3.5 transition-transform duration-200 motion-reduce:transition-none',
                    !isCollapsed && 'rotate-180',
                  )}
                />
              </CollapsibleTrigger>

              <CollapsibleContent>{GroupContent}</CollapsibleContent>
            </SidebarGroup>
          </Collapsible>
        );
      })}

      {groups.length === 0 && (
        <p role="status" className="px-3 py-4 text-sm text-muted-foreground">
          {t("AxiomBase:NoSettingsFound")}.
        </p>
      )}
    </aside>
  );
}
