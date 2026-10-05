import { useCallback, useEffect, useRef, useSyncExternalStore } from 'react';
import { BookTextIcon, HelpSquareIcon } from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';
import { type Nav, useNavGroups } from '@axiomframework/react-core';

import {
  Sidebar,
  SidebarHeader,
  SidebarMenu,
  SidebarFooter,
  SidebarSeparator,
  useSidebar,
} from '../ui/sidebar';

import { usePreferences } from '../preferences/use-preferences';
import { CurrentUserDropdown } from '../current-user/current-user-dropdown';

import { TENANTS } from './data';
import { SidebarHeaderSearch } from './sidebar-header-search';
import { TenantSwitcher } from './tenant-switcher';
import { NavPanel } from './nav-panel';
import { NavItemNode } from './nav-node';

// Same name used inside ui/sidebar.tsx
const SIDEBAR_COOKIE_NAME = 'sidebar_state';

const readCookie = () => {
  const m = document.cookie.match(
    new RegExp(`(?:^|;\\s*)${SIDEBAR_COOKIE_NAME}=([^;]*)`),
  );
  return m ? m[1] : null;
};

// null → delete the cookie, otherwise restore it verbatim.
const restoreCookie = (value: string | null) => {
  document.cookie = value === null
    ? `${SIDEBAR_COOKIE_NAME}=; path=/; max-age=0`
    : `${SIDEBAR_COOKIE_NAME}=${value}; path=/`;
};

const RESOURCE_LINKS: readonly Nav[] = [
  {
    title: 'AxiomBase:Support',
    icon: <HugeiconsIcon icon={HelpSquareIcon} strokeWidth={2} />,
    url: 'https://discord.gg/vHxVJd9Bx',
  },
  {
    title: 'AxiomBase:Documents',
    icon: <HugeiconsIcon icon={BookTextIcon} strokeWidth={2} />,
    url: 'https://axiomframework.dev/get-started/overview',
  },
];

export function AppSidebar(props: React.ComponentProps<typeof Sidebar>) {
  const pathname = useSyncExternalStore(
    subscribe,
    () => window.location.pathname,
    () => '',
  );
  const preferences = usePreferences((state) => state.preferences);
  const groups = useNavGroups();

  const { open, setOpen, isMobile } = useSidebar();

  const hoverOpened = useRef(false);
  const closeTimer = useRef<number | null>(null);

  const handleMouseEnter = useCallback(() => {
    if (isMobile || open) return;
    if (closeTimer.current) window.clearTimeout(closeTimer.current);

    // Remember the persisted value, then undo whatever setOpen writes.
    const snapshot = readCookie();
    hoverOpened.current = true;
    setOpen(true);
    restoreCookie(snapshot);
  }, [isMobile, open, setOpen]);

  const handleMouseLeave = useCallback(() => {
    if (isMobile || !hoverOpened.current) return;

    if (closeTimer.current) window.clearTimeout(closeTimer.current);
    closeTimer.current = window.setTimeout(() => {
      hoverOpened.current = false;
      const snapshot = readCookie(); // still the original (hover never persisted)
      setOpen(false);
      restoreCookie(snapshot);
    }, 100);
  }, [isMobile, setOpen]);

  useEffect(() => () => {
    if (closeTimer.current) window.clearTimeout(closeTimer.current);
  }, []);

  return (
    <Sidebar
      collapsible="icon"
      variant={preferences.sidebarStyle}
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
      {...props}
    >
      <SidebarHeader>
        <TenantSwitcher tenants={TENANTS} />
        <SidebarHeaderSearch />
      </SidebarHeader>

      <NavPanel groups={groups} pathname={pathname} />

      <div className="px-2">
        <SidebarSeparator className="mx-0" />
      </div>

      <SidebarFooter>
        <SidebarMenu>
          {RESOURCE_LINKS.map((item) => (
            <NavItemNode
              key={item.url ?? item.title}
              item={item}
              pathname={pathname}
              target="_blank"
            />
          ))}
        </SidebarMenu>

        {preferences.userMenuPosition === 'sidebar' && (
          <SidebarMenu>
            <CurrentUserDropdown placement="sidebar" />
          </SidebarMenu>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}

function subscribe(callback: () => void) {
  window.addEventListener('popstate', callback);
  return () => window.removeEventListener('popstate', callback);
}
