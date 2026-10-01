import { HugeiconsIcon } from '@hugeicons/react';
import {
  File01Icon,
  Layers01Icon,
  UsersRoundIcon,
  UserSettings01Icon,
} from '@hugeicons/core-free-icons';
import type { Nav } from '@axiomframework/react-core';

/** Demo navigation includes real destinations and empty test items. */
export const TEST_NAVIGATION: Nav = {
  title: 'Test',
  icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
  childrenDisplay: 'panel',
  children: [
    {
      title: 'Management',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
      childrenDisplay: 'panel',
      children: [
        { title: 'Users', url: '/identity-management/users', icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} /> },
        { title: 'Roles', url: '/identity-management/roles', icon: <HugeiconsIcon icon={UserSettings01Icon} strokeWidth={2} /> },
        { title: 'Tenants', url: '/saas-management/tenants', icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} /> },
      ],
    },
    {
      title: 'Identity Management',
      childrenDisplay: 'group',
      icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} />,
      children: [
        { title: 'Test Users', icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} /> },
        { title: 'Test Roles', icon: <HugeiconsIcon icon={UserSettings01Icon} strokeWidth={2} /> },
      ],
    },
    {
      title: 'Tenant Management',
      childrenDisplay: 'group',
      icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} />,
      children: [
        { title: 'Test Tenants', icon: <HugeiconsIcon icon={UsersRoundIcon} strokeWidth={2} /> },
        { title: 'Test Editions', icon: <HugeiconsIcon icon={Layers01Icon} strokeWidth={2} /> },
      ],
    },
    {
      title: 'Audit',
      childrenDisplay: 'group',
      icon: <HugeiconsIcon icon={File01Icon} strokeWidth={2} />,
      children: [
        { title: 'Test Audit Logs', icon: <HugeiconsIcon icon={File01Icon} strokeWidth={2} /> },
        { title: 'Test Entity Changes', icon: <HugeiconsIcon icon={File01Icon} strokeWidth={2} /> },
      ],
    },
  ],
};
