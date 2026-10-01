import { lazy } from 'react';
import { type Tab, appSettingsTabStore } from '@axiomframework/react-core';
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

const Sections = {
  // Account & Access
  general: lazy(() => import('./account-and-access/general-settings')),
  captcha: lazy(() => import('./account-and-access/captcha-settings')),
  idleSession: lazy(() => import('./account-and-access/idle-session-settings')),
  passkey: lazy(() => import('./account-and-access/passkey-settings')),
  emailing: lazy(() => import('./account-and-access/emailing-settings')),

  // Feature Management
  featureIdentity: lazy(() => import('./feature-management/identity-settings')),
  featureChat: lazy(() => import('./feature-management/chat-settings')),
  featureAudit: lazy(() => import('./feature-management/audit-settings')),
  featureLanguage: lazy(() => import('./feature-management/language-settings')),

  // Identity Management
  passwordPolicy: lazy(() => import('./identity-management/password-policy-settings')),
  lockout: lazy(() => import('./identity-management/lockout-settings')),
  verification: lazy(() => import('./identity-management/verification-settings')),
  userProfile: lazy(() => import('./identity-management/user-profile-settings')),
  sessions: lazy(() => import('./identity-management/sessions-settings')),

  // Tenant Management
  tenants: lazy(() => import('./tenant-management/tenants-settings')),
  editions: lazy(() => import('./tenant-management/editions-settings')),
} as const;

type GroupConfig = { title: string; tabs: Tab[] };

const GROUPS: GroupConfig[] = [
  {
    title: 'Account & Access',
    tabs: [
      {
        title: 'General',
        icon: <HugeiconsIcon icon={SettingsIcon} strokeWidth={2} />,
        component: Sections.general,
      },
      {
        title: 'Captcha',
        icon: <HugeiconsIcon icon={SecurityIcon} strokeWidth={2} />,
        component: Sections.captcha,
      },
      {
        title: 'Idle Session Timeout',
        icon: <HugeiconsIcon icon={TimeHalfPassIcon} strokeWidth={2} />,
        component: Sections.idleSession,
      },
      {
        title: 'Passkey',
        icon: <HugeiconsIcon icon={Key01Icon} strokeWidth={2} />,
        component: Sections.passkey,
      },
      {
        title: 'Emailing',
        icon: <HugeiconsIcon icon={MailIcon} strokeWidth={2} />,
        component: Sections.emailing,
      },
    ],
  },

  {
    title: 'Feature Management',
    tabs: [
      {
        title: 'Identity',
        icon: <HugeiconsIcon icon={UserRoundIcon} strokeWidth={2} />,
        component: Sections.featureIdentity,
      },
      {
        title: 'Chat',
        icon: <HugeiconsIcon icon={Chat01Icon} strokeWidth={2} />,
        component: Sections.featureChat,
      },
      {
        title: 'Audit Logging',
        icon: <HugeiconsIcon icon={FileTextIcon} strokeWidth={2} />,
        component: Sections.featureAudit,
      },
      {
        title: 'Language Management',
        icon: <HugeiconsIcon icon={LanguagesIcon} strokeWidth={2} />,
        component: Sections.featureLanguage,
      },
    ],
  },

  {
    title: 'Identity Management',
    tabs: [
      {
        title: 'Password Policy',
        icon: <HugeiconsIcon icon={LockPasswordIcon} strokeWidth={2} />,
        component: Sections.passwordPolicy,
      },
      {
        title: 'Lockout',
        icon: <HugeiconsIcon icon={LockKeyholeIcon} strokeWidth={2} />,
        component: Sections.lockout,
      },
      {
        title: 'Identity Verification',
        icon: <HugeiconsIcon icon={FaceIdIcon} strokeWidth={2} />,
        component: Sections.verification,
      },
      {
        title: 'User Profile',
        icon: <HugeiconsIcon icon={UserRoundIcon} strokeWidth={2} />,
        component: Sections.userProfile,
      },
      {
        title: 'Sessions',
        icon: <HugeiconsIcon icon={ClockIcon} strokeWidth={2} />,
        component: Sections.sessions,
      },
    ],
  },

  {
    title: 'Tenant Management',
    tabs: [
      {
        title: 'Tenants',
        icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
        component: Sections.tenants,
      },
      {
        title: 'Editions',
        icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
        component: Sections.editions,
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
