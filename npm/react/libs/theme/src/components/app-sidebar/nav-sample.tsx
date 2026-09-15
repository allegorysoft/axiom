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
import { AxiomNavManager } from '@axiomframework/react-core';

const PRODUCT = 'Product Management';
const ADMIN = 'Administration';
const USER = 'User';

AxiomNavManager.add({
  title: 'AxiomBase:Home',
  url: '/',
  icon: <Home />,
});

AxiomNavManager.addGroup(PRODUCT);
AxiomNavManager.add(
  {
    title: 'AxiomProductManagement:Dashboard',
    url: '/product-management/dashboard',
    icon: <LayoutDashboard />,
  },
  PRODUCT,
);
AxiomNavManager.add(
  {
    title: 'Products',
    icon: <List />,
  },
  PRODUCT,
);

AxiomNavManager.addGroup(ADMIN);
AxiomNavManager.add(
  {
    title: 'Tenant Management',
    url: '/tenant-management',
    icon: <Building2 />,
  },
  ADMIN,
);
AxiomNavManager.add(
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

AxiomNavManager.addGroup(USER);
AxiomNavManager.add(
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
