'use client';

import * as React from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import {
  UnfoldMoreIcon as ChevronsUpDownIcon,
  PlusIcon,
} from '@hugeicons/core-free-icons';

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from '../ui/sidebar';
import { Avatar, AvatarFallback } from '../ui/avatar';
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
  CommandSeparator,
} from '../ui/command';

// import { AddTenantDialog } from '../identity/saas-pages';

type Tenant = {
  id: string;
  name: string;
  logo: string;
  edition: string;
};

export function TenantSwitcher({ tenants }: { tenants: Tenant[] }) {
  const { isMobile, state } = useSidebar();
  const collapsedSidebar = !isMobile && state === 'collapsed';
  const [activeTenantId, setActiveTenantId] = React.useState(tenants[0]?.id);
  const activeTenant =
    tenants.find((tenant) => tenant.id === activeTenantId) ?? tenants[0];
  const [tenantQuery, setTenantQuery] = React.useState('');
  const [open, setOpen] = React.useState(false);

  const filteredTenants = tenants
    .filter((tenant) =>
      tenant.name.toLowerCase().includes(tenantQuery.trim().toLowerCase()),
    )
    .sort((a, b) => {
      if (a.id === activeTenant?.id) return -1;
      if (b.id === activeTenant?.id) return 1;
      return 0;
    });

  React.useEffect(() => {
    if (!open) {
      setTenantQuery('');
    }
  }, [open]);

  const [addingTenant, setAddingTenant] = React.useState(false);
  if (!activeTenant) return null;

  return (
    <>
      <SidebarMenu>
        <SidebarMenuItem>
          <DropdownMenu open={open} onOpenChange={setOpen}>
            <DropdownMenuTrigger
              render={
                collapsedSidebar ? (
                  <button
                    type="button"
                    aria-label={activeTenant.name}
                    title={activeTenant.name}
                    className="cursor-pointer rounded-full outline-none transition-transform active:scale-95"
                  />
                ) : (
                  <SidebarMenuButton
                    size="lg"
                    className="data-open:bg-sidebar-accent data-open:text-sidebar-accent-foreground"
                    tooltip={activeTenant.name}
                  />
                )
              }
            >
              <TenantAvatar tenant={activeTenant} />
              <div className="grid flex-1 text-left text-sm leading-tight group-data-[collapsible=icon]:hidden">
                <span className="truncate font-medium">
                  {activeTenant.name}
                </span>
                <span className="truncate text-xs">{activeTenant.edition}</span>
              </div>
              <HugeiconsIcon
                icon={ChevronsUpDownIcon}
                strokeWidth={2}
                className="ml-auto group-data-[collapsible=icon]:hidden"
              />
            </DropdownMenuTrigger>

            <DropdownMenuContent
              className="min-w-60"
              align="start"
              side={isMobile ? 'bottom' : 'right'}
              sideOffset={4}
            >
              <div onKeyDown={(e) => e.stopPropagation()}>
                <Command
                  data-tenant-switcher=""
                  key={open ? 'open' : 'closed'}
                  shouldFilter={false}
                  className="p-0 [&_[cmdk-group-items]]:space-y-1 [&_[data-slot=command-item]]:gap-2 [&_[data-slot=command-item]]:px-2 [&_[data-slot=command-item]]:py-2"
                >
                  <CommandInput
                    autoFocus
                    placeholder="Find Tenant"
                    value={tenantQuery}
                    onValueChange={setTenantQuery}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        setOpen(false);
                      }
                    }}
                  />
                  <CommandList className="max-h-64">
                    <CommandEmpty>No tenant found.</CommandEmpty>
                    <CommandGroup>
                      {filteredTenants.map((tenant) => (
                        <CommandItem
                          key={tenant.id}
                          value={tenant.name}
                          data-checked={tenant.id === activeTenant?.id}
                          className={
                            tenant.id === activeTenant?.id
                              ? 'bg-sidebar-accent text-sidebar-accent-foreground data-selected:bg-sidebar-accent data-selected:text-sidebar-accent-foreground data-selected:*:[svg]:text-sidebar-accent-foreground'
                              : 'data-selected:bg-sidebar-accent data-selected:text-sidebar-accent-foreground data-selected:*:[svg]:text-sidebar-accent-foreground'
                          }
                          onSelect={() => {
                            setActiveTenantId(tenant.id);
                            setOpen(false);
                          }}
                        >
                          <TenantAvatar
                            tenant={tenant}
                            className="size-6"
                          />
                          <span>{tenant.name}</span>
                        </CommandItem>
                      ))}
                    </CommandGroup>
                  </CommandList>
                  <CommandSeparator className="mx-1 my-1" />
                  <CommandGroup>
                    <CommandItem
                      onSelect={() => {
                        setOpen(false);
                        setAddingTenant(true);
                      }}
                    >
                      <HugeiconsIcon icon={PlusIcon} strokeWidth={2} />
                      Add Tenant
                    </CommandItem>
                  </CommandGroup>
                </Command>
              </div>
            </DropdownMenuContent>
          </DropdownMenu>
        </SidebarMenuItem>
      </SidebarMenu>
      {/*{addingTenant && (
        <AddTenantDialog onClose={() => setAddingTenant(false)} />
      )}*/}
    </>
  );
}

type TenantAvatarProps = { tenant: Tenant; className?: string };
function TenantAvatar({ tenant, className = 'size-8' }: TenantAvatarProps) {
  return (
    <Avatar className={className}>
      <AvatarFallback className="text-xs font-semibold">
        {tenant.logo}
      </AvatarFallback>
    </Avatar>
  );
}
