import { useUser } from '@axiomframework/react-core';

import { Separator } from '../ui/separator';
import { SidebarTrigger } from '../ui/sidebar';

import type { NavbarBehavior } from '../preferences/options';
import { PreferencesPopover } from '../preferences/preferences-popover';
import { usePreferences } from '../preferences/preferences-hooks';

import { ThemeToggle } from '../theme/theme-toggle';
import { UserDropdown } from '../user/user-dropdown';
import { LoginButton } from '../login-button';
import { Languages } from './languages';
import { AppTabsList } from './app-tabs-list';

const HEADER_CLASS_NAMES =
  'flex h-12 shrink-0 items-center gap-2 transition-[width,height] ease-linear group-has-data-[collapsible=icon]/sidebar-wrapper:h-12 px-3 md:px-4 backdrop-blur supports-[backdrop-filter]:bg-background/60 rounded-t-l';

const NAVBAR_BEHAVIOR_CLASS_NAMES: Record<NavbarBehavior, string> = {
  sticky: 'sticky top-2 z-50 mt-1 rounded-lg',
  scroll: 'border-b rounded-t-lg',
};

export function Header() {
  const user = useUser();
  const userMenuPosition = usePreferences().userMenuPosition;
  const navbarBehavior = usePreferences().navbarBehavior;

  const behaviorClassNames =
    NAVBAR_BEHAVIOR_CLASS_NAMES[navbarBehavior] ??
    NAVBAR_BEHAVIOR_CLASS_NAMES.sticky;

  return (
    <header
      className={`${HEADER_CLASS_NAMES} ${behaviorClassNames} border-b-0 rounded-b-none bg-card!`}
    >
      <SidebarTrigger className="-ml-1" />
      <Separator
        orientation="vertical"
        className="data-[orientation=vertical]:h-4"
      />

      <AppTabsList />

      <div className="ml-auto flex min-w-0 items-center gap-2">
        <PreferencesPopover />
        <ThemeToggle />
        <Languages />
        {userMenuPosition === 'navbar' && user === null && <LoginButton />}
        {userMenuPosition === 'navbar' && <UserDropdown />}
      </div>
    </header>
  );
}
