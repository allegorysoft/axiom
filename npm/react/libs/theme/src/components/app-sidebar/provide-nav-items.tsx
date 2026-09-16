import {
  LayoutDashboard,
  Settings,
  User,
  Shield,
  Cog,
  Server,
  Palette,
  List,
  Building2,
  Home,
} from 'lucide-react';
import { navStore } from '@axiomframework/react-core';

const PRODUCT = 'Product Management';
const ADMIN = 'Administration';
const USER = 'User';

export function provideNavItems() {
  navStore.add({
    title: 'AxiomBase:Home',
    url: '/',
    icon: <Home />,
  });

  navStore.addGroup(PRODUCT);
  navStore.add(
    {
      title: 'AxiomProductManagement:Dashboard',
      url: '/product-management/dashboard',
      icon: <LayoutDashboard />,
    },
    PRODUCT,
  );
  navStore.add(
    {
      title: 'Products',
      icon: <List />,
    },
    PRODUCT,
  );

  navStore.addGroup(ADMIN);
  navStore.add(
    {
      title: 'Tenant Management',
      url: '/tenant-management',
      icon: <Building2 />,
    },
    ADMIN,
  );
  navStore.add(
    {
      title: 'Setting Management',
      icon: <Cog />,
      children: [
        { title: 'Settings', url: '/setting-management' },
        { title: 'Profile', url: '/admin/settings/profile', icon: <Shield /> },
        { title: 'System', icon: <Server />, url: '/admin/settings/system' },
      ],
    },
    ADMIN,
  );

  navStore.addGroup(USER);
  navStore.add(
    {
      title: 'Settings',
      icon: <Settings />,
      children: [
        {
          title: 'Profile',
          url: '/user/settings/profile',
          icon: <User />,
        },
        {
          title: 'Preferences',
          icon: <Palette />,
          children: [
            {
              title: 'Theme',
              url: '/user/settings/preferences/theme',
              icon: <Palette />,
            },
          ],
        },
      ],
    },
    USER,
  );
}
