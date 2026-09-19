import { HugeiconsIcon } from '@hugeicons/react';
import { LayoutDashboardIcon as LayoutDashboard, SettingsIcon as Settings, ShieldIcon as Shield, CogIcon as Cog, ServerIcon as Server, PaletteIcon as Palette, ListIcon as List, Building02Icon as Building2, HomeIcon as Home } from '@hugeicons/core-free-icons';
import { navStore } from '@axiomframework/react-core';

const PRODUCT = 'Product Management';
const ADMIN = 'Administration';
const USER = 'User';

export function provideNavItems() {
  navStore.add({
    title: 'AxiomBase:Home',
    url: '/',
    icon: <HugeiconsIcon icon={Home} strokeWidth={2} />,
  });

  navStore.addGroup(PRODUCT);
  navStore.add(
    {
      title: 'Dashboard',
      url: '/product-management/dashboard',
      icon: <HugeiconsIcon icon={LayoutDashboard} strokeWidth={2} />,
    },
    PRODUCT,
  );
  navStore.add(
    {
      title: 'Products',
      url: '/product-management',
      icon: <HugeiconsIcon icon={List} strokeWidth={2} />,
    },
    PRODUCT,
  );

  navStore.addGroup(ADMIN);
  navStore.add(
    {
      title: 'Tenant Management',
      url: '/tenant-management',
      icon: <HugeiconsIcon icon={Building2} strokeWidth={2} />,
    },
    ADMIN,
  );
  navStore.add(
    {
      title: 'Setting Management',
      icon: <HugeiconsIcon icon={Cog} strokeWidth={2} />,
      children: [
        { title: 'Settings', url: '/setting-management' },
        { title: 'Profile', url: '/admin/settings/profile', icon: <HugeiconsIcon icon={Shield} strokeWidth={2} /> },
        { title: 'System', icon: <HugeiconsIcon icon={Server} strokeWidth={2} />, url: '/admin/settings/system' },
      ],
    },
    ADMIN,
  );

  navStore.addGroup(USER);
  navStore.add(
    {
      title: 'Settings',
      icon: <HugeiconsIcon icon={Settings} strokeWidth={2} />,
      children: [
        {
          title: 'Profile',
          url: '/user/settings/profile',
        },
        {
          title: 'Security',
          url: '/user/settings/security',
        },
        {
          title: 'Preferences',
          icon: <HugeiconsIcon icon={Palette} strokeWidth={2} />,
          children: [
            {
              title: 'Theme',
              url: '/user/settings/preferences/theme',
              icon: <HugeiconsIcon icon={Palette} strokeWidth={2} />,
            },
          ],
        },
      ],
    },
    USER,
  );
}
