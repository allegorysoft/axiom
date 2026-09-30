import { lazy } from 'react';
import { type Tab, profileTabStore } from '@axiomframework/react-core';
import {
  LanguagesIcon,
  Layout01Icon,
  PaletteIcon,
  SquareUserRoundIcon,
  TimeZoneIcon,
  UserAccountIcon,
  KeyRoundIcon,
  TextFontIcon,
} from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

const Sections = {
  profilePhoto: lazy(() => import('./account/profile-photo')),
  accountInfo: lazy(() => import('./account/account-info')),
  security: lazy(() => import('./account/security')),

  layout: lazy(() => import('./appearance/layout-settings')),
  theme: lazy(() => import('./appearance/theme-settings')),
  typography: lazy(() => import('./appearance/typography-settings')),

  language: lazy(() => import('./language-and-region/language-settings')),
  timeZone: lazy(() => import('./language-and-region/time-zone-settings')),
} as const;

type GroupConfig = { title: string; tabs: Tab[] };

const GROUPS: GroupConfig[] = [
  {
    title: 'Account',
    tabs: [
      {
        title: 'Profile',
        icon: <HugeiconsIcon icon={SquareUserRoundIcon} strokeWidth={2} />,
        component: Sections.profilePhoto,
      },
      {
        title: 'Account Details',
        icon: <HugeiconsIcon icon={UserAccountIcon} strokeWidth={2} />,
        component: Sections.accountInfo,
      },
      {
        title: 'Password & Security',
        icon: <HugeiconsIcon icon={KeyRoundIcon} strokeWidth={2} />,
        component: Sections.security,
      },
    ],
  },
  {
    title: 'Appearance',
    tabs: [
      {
        title: 'Layout',
        icon: <HugeiconsIcon icon={Layout01Icon} strokeWidth={2} />,
        component: Sections.layout,
      },
      {
        title: 'Theme',
        icon: <HugeiconsIcon icon={PaletteIcon} strokeWidth={2} />,
        component: Sections.theme,
      },
      {
        title: 'Typography',
        icon: <HugeiconsIcon icon={TextFontIcon} strokeWidth={2} />,
        component: Sections.typography,
      },
    ],
  },
  {
    title: 'Language & Region',
    tabs: [
      {
        title: 'Language',
        icon: <HugeiconsIcon icon={LanguagesIcon} strokeWidth={2} />,
        component: Sections.language,
      },
      {
        title: 'Time Zone',
        icon: <HugeiconsIcon icon={TimeZoneIcon} strokeWidth={2} />,
        component: Sections.timeZone,
      },
    ],
  },
];

export function provideProfileTabs() {
  for (const { title, tabs } of GROUPS) {
    profileTabStore.addGroup(title);
    for (const tab of tabs) {
      profileTabStore.add(tab, title);
    }
  }
}
