'use client';

import * as React from 'react';
import { ChevronsUpDownIcon, PlusIcon } from 'lucide-react';

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

type Tenant = {
  id: string;
  name: string;
  logo: string;
  edition: string;
};

export function TenantSwitcher({ tenants }: { tenants: Tenant[] }) {
  const { isMobile } = useSidebar();
  const [activeTenant, setActiveTenant] = React.useState(tenants[0]);
  const [tenantQuery, setTenantQuery] = React.useState('');
  const [open, setOpen] = React.useState(false);

  if (!activeTenant) {
    return null;
  }

  const filteredTenants = tenants
    .filter((tenant) =>
      tenant.name.toLowerCase().includes(tenantQuery.trim().toLowerCase()),
    )
    .sort((a, b) => {
      if (a.id === activeTenant.id) return -1;
      if (b.id === activeTenant.id) return 1;
      return 0;
    });

  React.useEffect(() => {
    if (!open) {
      setTenantQuery('');
    }
  }, [open]);

  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <DropdownMenu open={open} onOpenChange={setOpen}>
          <DropdownMenuTrigger
            render={
              <SidebarMenuButton
                size="lg"
                className="data-open:bg-sidebar-accent data-open:text-sidebar-accent-foreground"
              />
            }
          >
            <TenantAvatar tenant={activeTenant} />
            <div className="grid flex-1 text-left text-sm leading-tight">
              <span className="truncate font-medium">{activeTenant.name}</span>
              <span className="truncate text-xs">{activeTenant.edition}</span>
            </div>
            <ChevronsUpDownIcon className="ml-auto" />
          </DropdownMenuTrigger>

          <DropdownMenuContent
            className="min-w-60"
            align="start"
            side={isMobile ? 'bottom' : 'right'}
            sideOffset={4}
          >
            <div onKeyDown={(e) => e.stopPropagation()}>
              <Command
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
                <CommandList className="max-h-none">
                  <CommandEmpty>No tenant found.</CommandEmpty>
                  <CommandGroup>
                    {filteredTenants.map((tenant) => (
                      <CommandItem
                        key={tenant.id}
                        value={tenant.name}
                        data-checked={tenant.id === activeTenant.id}
                        className={
                          tenant.id === activeTenant.id
                            ? 'bg-sidebar-accent text-sidebar-accent-foreground data-selected:bg-sidebar-accent'
                            : undefined
                        }
                        onSelect={() => {
                          setActiveTenant(tenant);
                          setOpen(false);
                        }}
                      >
                        <TenantAvatar tenant={tenant} className="size-5 p-3" />
                        <span>{tenant.name}</span>
                      </CommandItem>
                    ))}
                  </CommandGroup>
                  <CommandSeparator className="mx-1 my-1" />
                  <CommandGroup>
                    <CommandItem onSelect={() => setOpen(false)}>
                      <PlusIcon />
                      Add Tenant
                    </CommandItem>
                  </CommandGroup>
                </CommandList>
              </Command>
            </div>
          </DropdownMenuContent>
        </DropdownMenu>
      </SidebarMenuItem>
    </SidebarMenu>
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
