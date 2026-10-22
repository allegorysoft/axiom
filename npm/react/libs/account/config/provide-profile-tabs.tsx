import { lazy } from 'react';

import {
  KeyRoundIcon,
  SquareUserRoundIcon,
  UserAccountIcon,
} from '@hugeicons/core-free-icons';
import { HugeiconsIcon } from '@hugeicons/react';

import { profileTabStore } from '@axiomframework/react-core';
import { ProfileTabKeys } from './enums/profile-tab-keys';

export function provideProfileTabs() {
  profileTabStore.addGroup(ProfileTabKeys.Account, {
    mode: 'collapsible',
    isActive: false,
  });

  profileTabStore.add(
    {
      title: ProfileTabKeys.Profile,
      icon: <HugeiconsIcon icon={SquareUserRoundIcon} strokeWidth={2} />,
      component: lazy(() => import('./components/profile-photo')),
    },
    ProfileTabKeys.Account,
  );

  profileTabStore.add(
    {
      title: ProfileTabKeys.AccountDetails,
      icon: <HugeiconsIcon icon={UserAccountIcon} strokeWidth={2} />,
      component: lazy(() => import('./components/account-info')),
    },
    ProfileTabKeys.Account,
  );

  profileTabStore.add(
    {
      title: ProfileTabKeys.PasswordAndSecurity,
      icon: <HugeiconsIcon icon={KeyRoundIcon} strokeWidth={2} />,
      component: lazy(() => import('./components/security')),
    },
    ProfileTabKeys.Account,
  );
}
