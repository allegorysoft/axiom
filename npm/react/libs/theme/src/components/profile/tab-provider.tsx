import { lazy } from 'react';
import { type TabGroup, profileTabStore } from '@axiomframework/react-core';
import {
  LanguagesIcon,
  Layout01Icon,
  PaletteIcon,
  TimeZoneIcon,
  TextFontIcon,
} from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

const GROUPS: TabGroup[] = [
  {
    title: 'Appearance',
    children: [
      {
        title: 'Layout',
        icon: <HugeiconsIcon icon={Layout01Icon} strokeWidth={2} />,
        component: lazy(() => import('./appearance/layout-settings')),
      },
      {
        title: 'Theme',
        icon: <HugeiconsIcon icon={PaletteIcon} strokeWidth={2} />,
        component: lazy(() => import('./appearance/theme-settings')),
      },
      {
        title: 'Typography',
        icon: <HugeiconsIcon icon={TextFontIcon} strokeWidth={2} />,
        component: lazy(() => import('./appearance/typography-settings')),
      },
    ],
  },
  {
    title: 'Language & Region',
    children: [
      {
        title: 'Language',
        icon: <HugeiconsIcon icon={LanguagesIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./language-and-region/language-settings'),
        ),
      },
      {
        title: 'Time Zone',
        icon: <HugeiconsIcon icon={TimeZoneIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./language-and-region/time-zone-settings'),
        ),
      },
    ],
  },
];

export function provideProfileTabs() {
  for (const { title, children, isActive, mode } of GROUPS) {
    profileTabStore.addGroup(title, { isActive, mode });
    for (const tab of children) {
      profileTabStore.add(tab, title);
    }
  }
}
