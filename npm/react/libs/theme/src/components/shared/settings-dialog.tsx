import { useLayoutEffect, useRef, useState } from 'react';
import { cn } from 'cn';
import {
  SearchIcon,
  ChevronDownIcon,
  ChevronRightIcon,
  XIcon,
} from '@hugeicons/core-free-icons';
import { HugeiconsIcon, type IconSvgElement } from '@hugeicons/react';

import {
  type TabGroup,
  getAvatarFallbackText,
  useUser,
} from '@axiomframework/react-core';

import { Button } from '../ui/button';
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
  Dialog,
  DialogContent,
  DialogClose,
  DialogDescription,
  DialogTitle,
} from '../ui/dialog';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from '../ui/sidebar';

import type { SettingsDialogProps, DialogContentHeaderProps } from './models';

export function SettingsDialog({
  title,
  description,
  open,
  onOpenChange,

  groups,
  group,
  tab,
  onTabChange,

  finalFocus,
}: SettingsDialogProps) {
  const user = useUser((state) => state);

  const [query, setQuery] = useState('');
  const [_, setCollapsedGroups] = useState<string[]>([]);
  const dialogRef = useRef<HTMLDivElement>(null);
  const wasOpen = useRef(false);

  useLayoutEffect(() => {
    if (open && !wasOpen.current) {
      setQuery('');
      setCollapsedGroups([]);
      onTabChange(tab, group);
    }

    wasOpen.current = open;
  }, [open, groups, onTabChange]);

  const Content = tab?.component;
  const search = query.trim().toLocaleLowerCase();
  const visibleGroups: readonly TabGroup[] = groups.filter((group) =>
    group.children.some((item) =>
      [item.title, group.title].some((label) =>
        label.toLocaleLowerCase().includes(search),
      ),
    ),
  );

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        ref={dialogRef}
        initialFocus={dialogRef}
        tabIndex={-1}
        finalFocus={finalFocus}
        showCloseButton={false}
        className="h-[min(760px,90dvh)] max-w-[calc(100%-1rem)] gap-0 overflow-hidden bg-background p-0 sm:max-w-5xl"
      >
        <DialogTitle className="sr-only">{title}</DialogTitle>
        <DialogDescription className="sr-only">{description}</DialogDescription>

        <SidebarProvider className="h-full min-h-0 flex-col items-stretch md:flex-row">
          <Sidebar
            collapsible="offcanvas"
            className="h-auto w-full shrink-0 border-b md:h-full md:w-60 md:border-r md:border-b-0"
          >
            <SidebarHeader className="gap-3 p-3">
              <div className="hidden items-center gap-3 md:flex">
                <Avatar className="size-10">
                  <AvatarImage src={user.photo || undefined} alt={user.name} />
                  <AvatarFallback>
                    {getAvatarFallbackText(user.name)}
                  </AvatarFallback>
                </Avatar>

                <div className="min-w-0">
                  <p className="truncate text-sm font-semibold">{user.name}</p>
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
                  onChange={(event) => {
                    setQuery(event.target.value);
                    setCollapsedGroups([]);
                  }}
                />
                <InputGroupAddon>
                  <HugeiconsIcon icon={SearchIcon} strokeWidth={2} />
                </InputGroupAddon>
              </InputGroup>
            </SidebarHeader>

            <SidebarContent className="max-h-[40dvh] px-3 pb-3 pt-0 [scrollbar-color:var(--muted-foreground)_transparent] [scrollbar-width:thin] md:max-h-none">
              <DialogNav
                groups={visibleGroups}
                tab={tab}
                onTabChange={onTabChange}
              />
            </SidebarContent>
          </Sidebar>

          <div className="flex min-h-0 min-w-0 flex-1 flex-col">
            <DialogContentHeader title={title} active={tab} group={group} />

            <section
              key={tab.title}
              aria-label={tab.title}
              className="min-h-0 flex-1 overflow-y-auto overscroll-contain [scrollbar-color:var(--muted-foreground)_transparent] [scrollbar-width:thin]"
            >
              <div className="mx-auto max-w-3xl px-6 py-8 md:px-10 md:py-10">
                <div className="mb-8 space-y-2">
                  <h2 className="text-2xl font-semibold tracking-tight">
                    {tab.title}
                  </h2>
                  <p className="text-sm text-muted-foreground">{tab.title}</p>
                </div>

                <Content />
              </div>
            </section>
          </div>
        </SidebarProvider>
      </DialogContent>
    </Dialog>
  );
}

function DialogNav({
  groups,
  tab,
  onTabChange,
}: Pick<SettingsDialogProps, 'groups' | 'tab' | 'onTabChange'>) {
  const [collapsedGroups, setCollapsedGroups] = useState<TabGroup[]>([]);

  return (
    <nav aria-label="User settings" className="flex flex-col">
      {groups.map((group) => {
        const collapsed = collapsedGroups.includes(group);

        return (
          <Collapsible
            key={group.title}
            open={!collapsed}
            onOpenChange={(open) =>
              setCollapsedGroups((current) =>
                open
                  ? current.filter((item) => item !== group)
                  : [...current, group],
              )
            }
          >
            <SidebarGroup className="mb-4 w-full p-0">
              <CollapsibleTrigger className="flex h-8 w-full cursor-pointer items-center justify-between rounded-md px-3 text-xs font-medium uppercase text-muted-foreground outline-none hover:text-sidebar-foreground focus-visible:ring-2 focus-visible:ring-sidebar-ring">
                {group.title}

                <HugeiconsIcon
                  icon={ChevronDownIcon}
                  strokeWidth={2}
                  className={cn(
                    'size-3.5 transition-transform duration-200 motion-reduce:transition-none',
                    !collapsed && 'rotate-180',
                  )}
                />
              </CollapsibleTrigger>

              <CollapsibleContent>
                <SidebarGroupContent>
                  <SidebarMenu className="gap-1">
                    {group.children.map((item, index) => {
                      const Icon = item.icon;

                      return (
                        <SidebarMenuItem key={item?.title || index}>
                          <SidebarMenuButton
                            type="button"
                            isActive={item.title === tab.title}
                            aria-current={item.title === tab.title}
                            onClick={() => onTabChange(item, group)}
                            className="h-9 gap-3 px-3 transition-colors"
                          >
                            <HugeiconsIcon
                              icon={Icon as IconSvgElement}
                              strokeWidth={2}
                            />
                            <span>{item.title}</span>
                          </SidebarMenuButton>
                        </SidebarMenuItem>
                      );
                    })}
                  </SidebarMenu>
                </SidebarGroupContent>
              </CollapsibleContent>
            </SidebarGroup>
          </Collapsible>
        );
      })}

      {groups.length === 0 && (
        <p role="status" className="px-3 py-4 text-sm text-muted-foreground">
          No settings found.
        </p>
      )}
    </nav>
  );
}

function DialogContentHeader({
  title,
  active,
  group,
}: DialogContentHeaderProps & { group: TabGroup }) {
  return (
    <header className="flex shrink-0 items-center border-b py-3 px-3 ">
      <SidebarTrigger />

      <span className="text-sm text-muted-foreground ml-2">{title}</span>
      <HugeiconsIcon
        icon={ChevronRightIcon}
        strokeWidth={2}
        aria-hidden="true"
        className="mx-3 size-4 shrink-0 text-muted-foreground/50"
      />

      <span className="text-sm text-muted-foreground">{group.title}</span>
      <HugeiconsIcon
        icon={ChevronRightIcon}
        strokeWidth={2}
        aria-hidden="true"
        className="mx-3 size-4 shrink-0 text-muted-foreground/50"
      />
      <span className="text-sm font-medium">{active.title}</span>

      <DialogClose
        render={
          <Button variant="ghost" size="icon" className="ml-auto shrink-0" />
        }
        aria-label="Close settings"
      >
        <HugeiconsIcon icon={XIcon} strokeWidth={2} />
      </DialogClose>
    </header>
  );
}
