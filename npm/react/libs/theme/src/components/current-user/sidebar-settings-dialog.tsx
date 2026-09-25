import { useUserProfile } from './user-profile-store';
import {
  useLayoutEffect,
  useRef,
  useState,
  type ComponentProps,
  type ComponentType,
} from 'react';
import {
  SearchIcon,
  ChevronDownIcon,
  ChevronRightIcon,
  XIcon,
} from '@hugeicons/core-free-icons';
import { HugeiconsIcon, type IconSvgElement } from '@hugeicons/react';
import { getAvatarFallbackText } from '@axiomframework/react-core';
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
} from '../ui/sidebar';

export type SettingsSection = {
  id: string;
  label: string;
  group: string;
  description: string;
  icon: IconSvgElement;
  component: ComponentType;
};

type Props = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  section: string;
  onSectionChange: (section: string) => void;
  title: string;
  description: string;
  sections: readonly SettingsSection[];
  finalFocus: ComponentProps<typeof DialogContent>['finalFocus'];
};

export function SidebarSettingsDialog({
  open,
  onOpenChange,
  section,
  onSectionChange,
  finalFocus,
  title,
  description,
  sections,
}: Props) {
  const user = useUserProfile((state) => state);
  const [query, setQuery] = useState('');
  const [collapsedGroups, setCollapsedGroups] = useState<string[]>([]);
  const dialogRef = useRef<HTMLDivElement>(null);
  const wasOpen = useRef(false);

  useLayoutEffect(() => {
    if (open && !wasOpen.current) {
      setQuery('');
      setCollapsedGroups([]);
      onSectionChange(sections[0].id);
    }

    wasOpen.current = open;
  }, [open, sections, onSectionChange]);

  const active = sections.find((item) => item.id === section) ?? sections[0];
  const Content = active.component;
  const search = query.trim().toLocaleLowerCase();
  const visibleSections = sections.filter((item) =>
    [item.label, item.group].some((label) =>
      label.toLocaleLowerCase().includes(search),
    ),
  );
  const groups = [...new Set(visibleSections.map((item) => item.group))];

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
            collapsible="none"
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
              <nav aria-label="User settings" className="flex flex-col">
                {groups.map((group) => (
                  <Collapsible
                    key={group}
                    open={!collapsedGroups.includes(group)}
                    onOpenChange={(nextOpen) =>
                      setCollapsedGroups((current) =>
                        nextOpen
                          ? current.filter((item) => item !== group)
                          : [...current, group],
                      )
                    }
                  >
                    <SidebarGroup className="mb-4 w-full p-0">
                      <CollapsibleTrigger className="flex h-8 w-full cursor-pointer items-center justify-between rounded-md px-3 text-xs font-medium uppercase text-muted-foreground outline-none hover:text-sidebar-foreground focus-visible:ring-2 focus-visible:ring-sidebar-ring">
                        {group}
                        <HugeiconsIcon
                          icon={ChevronDownIcon}
                          strokeWidth={2}
                          className={`size-3.5 transition-transform duration-200 motion-reduce:transition-none ${collapsedGroups.includes(group) ? '' : 'rotate-180'}`}
                        />
                      </CollapsibleTrigger>
                      <CollapsibleContent>
                        <SidebarGroupContent>
                          <SidebarMenu className="gap-1">
                            {visibleSections
                              .filter((item) => item.group === group)
                              .map((item) => (
                                <SidebarMenuItem key={item.id}>
                                  <SidebarMenuButton
                                    type="button"
                                    isActive={item.id === section}
                                    aria-current={
                                      item.id === section ? 'page' : undefined
                                    }
                                    onClick={() => onSectionChange(item.id)}
                                    className="h-9 gap-3 px-3 transition-colors"
                                  >
                                    <HugeiconsIcon
                                      icon={item.icon}
                                      strokeWidth={2}
                                    />
                                    <span>{item.label}</span>
                                  </SidebarMenuButton>
                                </SidebarMenuItem>
                              ))}
                          </SidebarMenu>
                        </SidebarGroupContent>
                      </CollapsibleContent>
                    </SidebarGroup>
                  </Collapsible>
                ))}
                {visibleSections.length === 0 && (
                  <p
                    role="status"
                    className="px-3 py-4 text-sm text-muted-foreground"
                  >
                    No settings found.
                  </p>
                )}
              </nav>
            </SidebarContent>
          </Sidebar>

          <div className="flex min-h-0 min-w-0 flex-1 flex-col">
            <header className="flex shrink-0 items-center border-b py-3 pl-6 pr-3">
              <span className="text-sm text-muted-foreground">{title}</span>
              <HugeiconsIcon
                icon={ChevronRightIcon}
                strokeWidth={2}
                aria-hidden="true"
                className="mx-3 size-4 shrink-0 text-muted-foreground/50"
              />

              <span className="text-sm text-muted-foreground">
                {active.group}
              </span>
              <HugeiconsIcon
                icon={ChevronRightIcon}
                strokeWidth={2}
                aria-hidden="true"
                className="mx-3 size-4 shrink-0 text-muted-foreground/50"
              />
              <span className="text-sm font-medium">{active.label}</span>
              <DialogClose
                render={
                  <Button
                    variant="ghost"
                    size="icon"
                    className="ml-auto shrink-0"
                  />
                }
                aria-label="Close settings"
              >
                <HugeiconsIcon icon={XIcon} strokeWidth={2} />
              </DialogClose>
            </header>
            <section
              key={section}
              aria-label={active.label}
              className="min-h-0 flex-1 overflow-y-auto overscroll-contain [scrollbar-color:var(--muted-foreground)_transparent] [scrollbar-width:thin]"
            >
              <div className="mx-auto max-w-3xl px-6 py-8 md:px-10 md:py-10">
                <div className="mb-8 space-y-2">
                  <h2 className="text-2xl font-semibold tracking-tight">
                    {active.label}
                  </h2>
                  <p className="text-sm text-muted-foreground">
                    {active.description}
                  </p>
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
