import {
  GalleryVerticalEndIcon,
  AudioLinesIcon,
  TerminalIcon,
  TerminalSquareIcon,
  BotIcon,
  BookOpenIcon,
  Settings2Icon,
  FrameIcon,
  PieChartIcon,
  MapIcon,
  Building2,
  Settings,
} from 'lucide-react';
import { Nav, NavGroup } from '@axiomframework/react-core';

export const DATA = {
  user: {
    name: 'masumulu',
    email: 'masum@allegorysoft.com',
    avatar: '/images/masum.jpg',
  },
  teams: [
    {
      name: 'Allegorysoft',
      logo: <GalleryVerticalEndIcon />,
      plan: 'Enterprise',
    },
    {
      name: 'Acme Corp.',
      logo: <AudioLinesIcon />,
      plan: 'Startup',
    },
    {
      name: 'Evil Corp.',
      logo: <TerminalIcon />,
      plan: 'Free',
    },
  ],
  tenants: [
    {
      id: '10ccc3c2-34c4-413b-8387-12f66519cd1d',
      name: 'Allegorysoft',
      logo: 'AS',
      edition: 'Enterprise',
    },
    {
      id: '6ccda1c1-ba31-4407-b804-51fb7384d8c8',
      name: 'Microsoft',
      logo: 'MS',
      edition: 'Enterprise',
    },
    {
      id: 'ecd3f3e3-854e-4656-b5c1-ae2f7d44fd92',
      name: 'GitHub',
      logo: 'GH',
      edition: 'Free',
    },
    {
      id: 'c3c254f1-5b53-43c4-852d-a037a55320a1',
      name: 'Apple',
      logo: 'AP',
      edition: 'Standard',
    },
    {
      id: 'd847c70d-a4fa-4c03-a295-6bb647513b15',
      name: 'Harbor Systems',
      logo: 'HS',
      edition: 'Professional',
    },
  ],
  navMain: [
    {
      title: 'Playground',
      url: '#',
      icon: <TerminalSquareIcon />,
      isActive: false,
      items: [
        {
          title: 'History',
          url: '#',
        },
        {
          title: 'Starred',
          url: '#',
        },
        {
          title: 'Settings',
          url: '#',
        },
      ],
    },
    {
      title: 'Models',
      url: '#',
      icon: <BotIcon />,
      items: [
        {
          title: 'Genesis',
          url: '#',
        },
        {
          title: 'Explorer',
          url: '#',
        },
        {
          title: 'Quantum',
          url: '#',
        },
      ],
    },
    {
      title: 'Documentation',
      url: '#',
      icon: <BookOpenIcon />,
      items: [
        {
          title: 'Introduction',
          url: '#',
        },
        {
          title: 'Get Started',
          url: '#',
        },
        {
          title: 'Tutorials',
          url: '#',
        },
        {
          title: 'Changelog',
          url: '#',
        },
      ],
    },
    {
      title: 'Settings',
      url: '#',
      icon: <Settings2Icon />,
      items: [
        {
          title: 'General',
          url: '#',
        },
        {
          title: 'Team',
          url: '#',
        },
        {
          title: 'Billing',
          url: '#',
        },
        {
          title: 'Limits',
          url: '#',
        },
      ],
    },
  ],
  projects: [
    {
      name: 'Design Engineering',
      url: '#',
      icon: <FrameIcon />,
    },
    {
      name: 'Sales & Marketing',
      url: '#',
      icon: <PieChartIcon />,
    },
    {
      name: 'Travel',
      url: '#',
      icon: <MapIcon />,
    },
  ],
};

const NAV_ITEMS: Nav[] = [
  {
    title: 'Tenant Management',
    icon: <Building2 />,
    isActive: false,
    children: [
      {
        title: 'Tenants',
        url: '/tenant-management',
      },
      {
        title: 'Edition parent',
        children: [{ title: 'Editions', url: '/tenant-management/editions' }],
      },
    ],
  },
  {
    title: 'Setting Management',
    icon: <Settings />,
    children: [
      {
        title: 'Settings',
        url: '/setting-management',
      },
    ],
  },
];

export const NAV_GROUPS: NavGroup[] = [
  { title: 'Analytics', isActive: false, items: [] },
  { title: 'Administrator', isActive: false, items: NAV_ITEMS },
];
