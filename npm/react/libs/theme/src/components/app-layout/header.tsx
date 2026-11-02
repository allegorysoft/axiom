import { useUser } from '@axiomframework/react-core';

import { Separator } from '../ui/separator';
import { SidebarTrigger } from '../ui/sidebar';

import type { NavbarBehavior } from '../preferences/options';
import { PreferencesPopover } from '../preferences/preferences-popover';
import { useLayout, usePreferences } from '../preferences/preferences-hooks';

import { WorkspaceTabs } from '../workspace-tab/workspace-tabs';

import { ThemeToggle } from '../theme/theme-toggle';
import { UserDropdown } from '../user/user-dropdown';
import { LoginButton } from '../login-button';
import { Languages } from './languages';

const HEADER_CLASS_NAMES = `flex h-12 px-3 md:px-4 items-center gap-2 backdrop-blur border m-1 mt-2 rounded-lg`;
const NAVBAR_BEHAVIOR_CLASS_NAMES: Record<NavbarBehavior, string> = {
  sticky: 'sticky top-1 z-50',
  scroll: '',
};

export function Header() {
  const user = useUser();
  const userMenuPosition = usePreferences().userMenuPosition;
  const navbarBehavior = usePreferences().navbarBehavior;
  const layout = useLayout();

  const behaviorClassNames =
    NAVBAR_BEHAVIOR_CLASS_NAMES[navbarBehavior] ??
    NAVBAR_BEHAVIOR_CLASS_NAMES.sticky;

  return (
    <header className={`${HEADER_CLASS_NAMES} ${behaviorClassNames}`}>
      <SidebarTrigger className="-ml-1" />
      <Separator
        orientation="vertical"
        className="data-[orientation=vertical]:h-4"
      />

      {layout !== 'default' && <WorkspaceTabs />}

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
