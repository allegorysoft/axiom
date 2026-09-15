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
      id: '1234-5678-9010',
      name: 'Allegorysoft',
      logo: 'AS',
      edition: 'Enterprise',
    },
    {
      id: '0987-6543-2109',
      name: 'GitHub',
      logo: 'GH',
      edition: 'Enterprise',
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
