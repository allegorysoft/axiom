import { lazy } from 'react';

import {
  Chat01Icon,
  ClockIcon,
  FaceIdIcon,
  FileTextIcon,
  Key01Icon,
  LanguagesIcon,
  Layers01Icon,
  LockKeyholeIcon,
  LockPasswordIcon,
  MailIcon,
  SecurityIcon,
  SettingsIcon,
  TimeHalfPassIcon,
  UserRoundIcon,
  UsersRoundIcon,
} from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import { type Tab, appSettingsTabStore } from '@axiomframework/react-core';

type GroupConfig = { title: string; tabs: Tab[] };

const GROUPS: GroupConfig[] = [
  {
    title: 'Account & Access',
    tabs: [
      {
        title: 'General',
        icon: <HugeiconsIcon icon={SettingsIcon} strokeWidth={2} />,
        component: lazy(() => import('./account-and-access/general-settings')),
      },
      {
        title: 'Captcha',
        icon: <HugeiconsIcon icon={SecurityIcon} strokeWidth={2} />,
        component: lazy(() => import('./account-and-access/captcha-settings')),
      },
      {
        title: 'Idle Session Timeout',
        icon: <HugeiconsIcon icon={TimeHalfPassIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./account-and-access/idle-session-settings'),
        ),
      },
      {
        title: 'Passkey',
        icon: <HugeiconsIcon icon={Key01Icon} strokeWidth={2} />,
        component: lazy(() => import('./account-and-access/passkey-settings')),
      },
      {
        title: 'Emailing',
        icon: <HugeiconsIcon icon={MailIcon} strokeWidth={2} />,
        component: lazy(() => import('./account-and-access/emailing-settings')),
      },
    ],
  },

  {
    title: 'Feature Management',
    tabs: [
      {
        title: 'Identity',
        icon: <HugeiconsIcon icon={UserRoundIcon} strokeWidth={2} />,
        component: lazy(() => import('./feature-management/identity-settings')),
      },
      {
        title: 'Chat',
        icon: <HugeiconsIcon icon={Chat01Icon} strokeWidth={2} />,
        component: lazy(() => import('./feature-management/chat-settings')),
      },
      {
        title: 'Audit Logging',
        icon: <HugeiconsIcon icon={FileTextIcon} strokeWidth={2} />,
        component: lazy(() => import('./feature-management/audit-settings')),
      },
      {
        title: 'Language Management',
        icon: <HugeiconsIcon icon={LanguagesIcon} strokeWidth={2} />,
        component: lazy(() => import('./feature-management/language-settings')),
      },
    ],
  },

  {
    title: 'Identity Management',
    tabs: [
      {
        title: 'Password Policy',
        icon: <HugeiconsIcon icon={LockPasswordIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./identity-management/password-policy-settings'),
        ),
      },
      {
        title: 'Lockout',
        icon: <HugeiconsIcon icon={LockKeyholeIcon} strokeWidth={2} />,
        component: lazy(() => import('./identity-management/lockout-settings')),
      },
      {
        title: 'Identity Verification',
        icon: <HugeiconsIcon icon={FaceIdIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./identity-management/verification-settings'),
        ),
      },
      {
        title: 'User Profile',
        icon: <HugeiconsIcon icon={UserRoundIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./identity-management/user-profile-settings'),
        ),
      },
      {
        title: 'Sessions',
        icon: <HugeiconsIcon icon={ClockIcon} strokeWidth={2} />,
        component: lazy(
          () => import('./identity-management/sessions-settings'),
        ),
      },
    ],
  },

  {
    title: 'Tenant Management',
    tabs: [
      {
        title: 'Tenants',
        icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
        component: lazy(() => import('./tenant-management/tenants-settings')),
      },
      {
        title: 'Editions',
        icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
        component: lazy(() => import('./tenant-management/editions-settings')),
      },
    ],
  },
];

export function provideAppSettingsTabs() {
  for (const { title, tabs } of GROUPS) {
    appSettingsTabStore.addGroup(title);
    for (const tab of tabs) {
      appSettingsTabStore.add(tab, title);
    }
  }
}
