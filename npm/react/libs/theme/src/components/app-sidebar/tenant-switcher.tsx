'use client';

import * as React from 'react';
import { Check, ChevronsUpDownIcon, PlusIcon, SearchIcon } from 'lucide-react';

import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '../ui/dropdown-menu';
import {
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from '../ui/sidebar';
import { Avatar, AvatarFallback } from '../ui/avatar';
import { Input } from '../ui/input';

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

  if (!activeTenant) {
    return null;
  }

  const filteredTenants = tenants.filter((tenant) =>
    tenant.name.toLowerCase().includes(tenantQuery.trim().toLowerCase()),
  );

  return (
    <SidebarMenu>
      <SidebarMenuItem>
        <DropdownMenu
          onOpenChange={(open) => {
            if (!open) setTenantQuery('');
          }}
        >
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
            className="min-w-56 px-2"
            align="start"
            side={isMobile ? 'bottom' : 'right'}
            sideOffset={4}
          >
            <div className="p-1 pb-2">
              <div className="relative">
                <SearchIcon className="pointer-events-none absolute top-1/2 left-2 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  value={tenantQuery}
                  onChange={(e) => setTenantQuery(e.target.value)}
                  onKeyDown={(e) => e.stopPropagation()}
                  placeholder="Find tenant"
                  className="h-8 pl-8"
                />
              </div>
            </div>

            <DropdownMenuGroup>
              {filteredTenants.length === 0 ? (
                <div className="px-2 py-1.5 text-sm text-muted-foreground">
                  No tenants found.
                </div>
              ) : (
                filteredTenants.map((tenant) => (
                  <DropdownMenuItem
                    key={tenant.name}
                    onClick={() => setActiveTenant(tenant)}
                    className={
                      'gap-2 mb-1 ' +
                      (tenant.name === activeTenant.name && 'bg-sidebar-accent')
                    }
                  >
                    <TenantAvatar tenant={tenant} />

                    {tenant.name}
                    {tenant.name === activeTenant.name && (
                      <Check className="ml-auto size-4 shrink-0" />
                    )}
                  </DropdownMenuItem>
                ))
              )}
            </DropdownMenuGroup>
            <DropdownMenuSeparator className="mx-1" />
            <DropdownMenuGroup>
              <DropdownMenuItem className="gap-2">
                <div className="flex size-7 items-center justify-center rounded-md border bg-transparent">
                  <PlusIcon className="size-4" />
                </div>
                <div className="font-medium text-muted-foreground">
                  Add tenant
                </div>
              </DropdownMenuItem>
            </DropdownMenuGroup>
          </DropdownMenuContent>
        </DropdownMenu>
      </SidebarMenuItem>
    </SidebarMenu>
  );
}

type TenantAvatarProps = { tenant: Tenant };
function TenantAvatar({ tenant }: TenantAvatarProps) {
  return (
    <Avatar className="size-7">
      <AvatarFallback className="text-xs font-semibold">
        {tenant.logo}
      </AvatarFallback>
    </Avatar>
  );
}
