import { useEffect, useState } from 'react';
import { HugeiconsIcon } from '@hugeicons/react';
import { Home03Icon as House } from '@hugeicons/core-free-icons';

import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from './ui/breadcrumb';
import { Separator } from './ui/separator';
import { SidebarTrigger } from './ui/sidebar';

import type { NavbarBehavior } from './preferences/options';
import { CurrentUserDropdown } from './current-user/current-user-dropdown';
import { Languages } from './languages';
import { ThemeToggle } from './theme/theme-toggle';
import { PreferencesPopover } from './preferences/preferences-popover';
import { usePreferences } from './preferences/use-preferences';

const HEADER_CLASS_NAMES =
  'flex h-14 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12 px-3 md:px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60 rounded-t-l';

const NAVBAR_BEHAVIOR_CLASS_NAMES: Record<NavbarBehavior, string> = {
  sticky: 'sticky top-2 z-50 border mx-2 my-2 rounded-lg',
  scroll: 'border-b rounded-t-lg',
};

export function Header() {
  const label = useDecodedHash();
  const userMenuPosition = usePreferences(
    (state) => state.preferences.userMenuPosition ?? 'navbar',
  );
  const navbarBehavior = usePreferences(
    (state) => state.preferences.navbarBehavior,
  );

  const behaviorClassNames =
    NAVBAR_BEHAVIOR_CLASS_NAMES[navbarBehavior] ??
    NAVBAR_BEHAVIOR_CLASS_NAMES.sticky;

  return (
    <header className={`${HEADER_CLASS_NAMES} ${behaviorClassNames}`}>
      <SidebarTrigger className="-ml-1" />
      <Separator
        orientation="vertical"
        className="mr-2 data-[orientation=vertical]:h-4"
      />
      <Breadcrumb>
        <BreadcrumbList>
          <BreadcrumbItem className="hidden md:block">
            <BreadcrumbLink href="/">
              <HugeiconsIcon icon={House} strokeWidth={2} className="size-4" />
            </BreadcrumbLink>
          </BreadcrumbItem>

          {label && (
            <>
              <BreadcrumbSeparator className="hidden md:block" />
              <BreadcrumbItem>
                <BreadcrumbPage>{label}</BreadcrumbPage>
              </BreadcrumbItem>
            </>
          )}
        </BreadcrumbList>
      </Breadcrumb>

      <div className="ml-auto flex min-w-0 items-center gap-2">
        <PreferencesPopover />
        <ThemeToggle />
        <Languages />
        {userMenuPosition === 'navbar' && <CurrentUserDropdown />}
      </div>
    </header>
  );
}

function decodeHash(hash: string): string {
  const raw = hash.startsWith('#') ? hash.slice(1) : hash;
  if (!raw) return '';

  try {
    return decodeURIComponent(raw);
  } catch {
    return raw;
  }
}

function useDecodedHash(): string {
  const [value, setValue] = useState(() =>
    typeof window === 'undefined' ? '' : decodeHash(window.location.hash),
  );

  useEffect(() => {
    const update = () => setValue(decodeHash(window.location.hash));

    update();
    window.addEventListener('hashchange', update);
    return () => window.removeEventListener('hashchange', update);
  }, []);

  return value;
}
